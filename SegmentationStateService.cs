using Segmentation.Client.Models;

namespace Segmentation.Client.Services;

/// <summary>
/// Service singleton réactif : quand les règles changent, toutes les pages
/// abonnées recalculent leurs statistiques automatiquement.
/// </summary>
public class SegmentationStateService
{
	private const int MinutesParAn = 220 * 8 * 60; // 105 600 min/an

	// Clients fixes par segment (mock réseau)
	private static readonly Dictionary<string, int> ClientsFixe = new()
	{
		["Faible Potentiel"] = 200_000,
		["Intermédiaire"] = 778_000,
		["Haut de gamme"] = 22_000,
	};

	// Tailles théoriques par défaut (pour calculer les ratios)
	private readonly Dictionary<string, int> _taillesDefaut = new();

	public event Action? OnChange;

	public ReglesHypothesesModel Regles { get; private set; }
	public List<TaillePortefeuille> TaillesPortefeuilles { get; private set; } = new();
	public int TotalConseillersCible { get; private set; }
	public int TaillePortefeuilleMoyenne { get; private set; }
	public List<DimensionnementEtpRow> EtpCibleRows { get; private set; } = new();

	public SegmentationStateService(MockDataService mock)
	{
		Regles = mock.GetReglesHypotheses();
		// Sauvegarder les tailles par défaut
		foreach (var pf in Regles.PortefeuillesTheoriques)
			_taillesDefaut[pf.Profil] = pf.ClientsParConseiller;
		Recalculer();
	}

	public void UpdateRegles(ReglesHypothesesModel regles)
	{
		Regles = regles;
		Recalculer();
		OnChange?.Invoke();
	}

	// Helpers : calculs Section 1+2 → taille théorique
	private double GetVolumeHoraire(string profil) =>
		Regles.ConseillersProfils.FirstOrDefault(p => p.Profil == profil) is { } cp
			? Math.Round(Regles.HeuresTravailParAn * cp.PartTempsCommercialPct / 100.0, 1)
			: 0;

	private double GetIntensite(string segment) =>
		Regles.SegmentsIntensite.FirstOrDefault(s => s.Segment == segment)?.IntensiteRelationnelle ?? 0;

	private int GetTailleTheorique(string profil, string segment)
	{
		var intensite = GetIntensite(segment);
		var volume = GetVolumeHoraire(profil);
		return intensite > 0 ? (int)Math.Round(volume / intensite) : 0;
	}

	private void Recalculer()
	{
		// ── Si nouveau modèle disponible : recalculer PortefeuillesTheoriques depuis Sections 1+2
		if (Regles.SegmentsIntensite.Any() && Regles.ConseillersProfils.Any())
		{
			foreach (var pf in Regles.PortefeuillesTheoriques)
			{
				pf.ClientsParConseiller = pf.Profil switch
				{
					"Portefeuilles Mutualisés" => GetTailleTheorique("CONSEILLER COMMERCIAL", "GP Standard"),
					"Dédiés" => GetTailleTheorique("CONSEILLER CLIENTELE", "CI Standard"),
					"Dédiés - Haut de Gamme" => GetTailleTheorique("BANQUIER PRIVÉ", "HDG Premium Potentiel"),
					_ => pf.ClientsParConseiller
				};
			}
		}
		else
		{
			// ── Fallback legacy
			double minutesDispo = MinutesParAn * (Regles.PartTempsCommercial / 100.0);
			var critIndex = Regles.CriteresApproche.ToDictionary(c => c.Segment);
			var profilToSeg = new Dictionary<string, string>
			{
				["Portefeuilles Mutualisés"] = "Faible Potentiel",
				["Dédiés"] = "Intermédiaire",
				["Dédiés - Haut de Gamme"] = "Haut de gamme",
			};
			foreach (var pf in Regles.PortefeuillesTheoriques)
			{
				if (profilToSeg.TryGetValue(pf.Profil, out var seg) &&
					critIndex.TryGetValue(seg, out var crit) &&
					crit.FrequenceRdvParAn > 0 && crit.DureeRdvMin > 0)
					pf.ClientsParConseiller = (int)(minutesDispo / (crit.FrequenceRdvParAn * crit.DureeRdvMin));
			}
		}

		var profilToSeg2 = new Dictionary<string, string>
		{
			["Portefeuilles Mutualisés"] = "Faible Potentiel",
			["Dédiés"] = "Intermédiaire",
			["Dédiés - Haut de Gamme"] = "Haut de gamme",
		};

		// Taille portefeuille par profil
		TaillesPortefeuilles = Regles.PortefeuillesTheoriques.Select(pf =>
		{
			var seg = profilToSeg2.GetValueOrDefault(pf.Profil, "");
			var clients = ClientsFixe.GetValueOrDefault(seg, 0);
			var etp = pf.ClientsParConseiller > 0 ? (double)clients / pf.ClientsParConseiller : 0;
			// ETP actuel fixe (basé sur 1 200 conseillers totaux proportionnellement)
			double etpActuel = pf.Profil switch
			{
				"Portefeuilles Mutualisés" => 150,
				"Dédiés" => 900,
				_ => 150,
			};
			double taux = etp > 0 ? Math.Min(etpActuel / etp * 100, 150) : 0;
			return new TaillePortefeuille
			{
				Profil = pf.Profil,
				ClientsParConseiller = pf.ClientsParConseiller,
				TauxRemplissage = Math.Round(taux, 1),
			};
		}).ToList();

		// ETP requis par profil (réseau entier)
		double etpMut = Etp("Portefeuilles Mutualisés", profilToSeg2);
		double etpDed = Etp("Dédiés", profilToSeg2);
		double etpHDG = Etp("Dédiés - Haut de Gamme", profilToSeg2);
		double totalEtp = etpMut + etpDed + etpHDG;

		TotalConseillersCible = (int)Math.Ceiling(totalEtp);
		TaillePortefeuilleMoyenne = TotalConseillersCible > 0
			? (int)(1_000_000.0 / TotalConseillersCible) : 0;

		// Facteur d'ajustement par rapport aux tailles par défaut
		double RatioEtp(string profil)
		{
			var defaut = _taillesDefaut.GetValueOrDefault(profil, 1);
			var actuel = Regles.PortefeuillesTheoriques.FirstOrDefault(p => p.Profil == profil)?.ClientsParConseiller ?? 1;
			return actuel > 0 ? (double)defaut / actuel : 1;
		}

		double rM = RatioEtp("Portefeuilles Mutualisés");
		double rD = RatioEtp("Dédiés");
		double rH = RatioEtp("Dédiés - Haut de Gamme");

		// Lignes ETP cible (par agence typique, en appliquant les ratios sur les valeurs mock)
		double m0 = Math.Round(0.08 * rM, 2), d0 = Math.Round(4.33 * rD, 2), h0 = Math.Round(0.67 * rH, 2);
		double chargeNC = Math.Round(0.15 * rD, 2);
		double chargeEtp = Math.Round(m0 + d0 + h0 + chargeNC, 2);
		double effCible = Math.Round(chargeEtp + 0.17, 2);
		const double effActuel = 6.00;

		EtpCibleRows = new List<DimensionnementEtpRow>
		{
			new() { Libelle = "Faible Potentiel",          Mutualise = m0,   Dedie = 0.02, DedieHautDeGamme = null, Total = Math.Round(m0 + 0.02, 2), Concordance = 92 },
			new() { Libelle = "Intermédiaires",             Mutualise = 0.03, Dedie = d0,   DedieHautDeGamme = 0.06, Total = Math.Round(d0 + 0.09, 2), Concordance = 97 },
			new() { Libelle = "Haut de Gamme",              Mutualise = null, Dedie = 0.06, DedieHautDeGamme = h0,   Total = Math.Round(h0 + 0.06, 2), Concordance = 97 },
			new() { Libelle = "Charge non-commerciale",     Mutualise = null, Dedie = chargeNC, DedieHautDeGamme = null, Total = Math.Round(chargeNC + 0.10, 2) },
			new() { Libelle = "Charge ETP",                 Mutualise = m0,   Dedie = Math.Round(d0 + chargeNC, 2), DedieHautDeGamme = h0, Total = chargeEtp, IsBold = true },
			new() { Libelle = "Effectif Cible",             Mutualise = Math.Round(m0 + 0.05, 2), Dedie = Math.Round(d0 + chargeNC + 0.07, 2), DedieHautDeGamme = Math.Round(h0 + 0.05, 2), Total = effCible, IsBold = true },
			new() { Libelle = "Effectif Cible – Charge ETP",Mutualise = 0.05, Dedie = 0.07, DedieHautDeGamme = 0.05, Total = Math.Round(effCible - chargeEtp, 2), IsEcart = true },
			new() { Libelle = "Effectif Actuel",            Mutualise = 0.78, Dedie = 4.45, DedieHautDeGamme = 0.78, Total = effActuel, IsBold = true },
			new() { Libelle = "Écart : Cible – Actuel",     Mutualise = Math.Round(m0 + 0.05 - 0.78, 2), Dedie = Math.Round(d0 + chargeNC + 0.07 - 4.45, 2), DedieHautDeGamme = Math.Round(h0 + 0.05 - 0.78, 2), Total = Math.Round(effCible - effActuel, 2), IsEcart = true },
			new() { Libelle = "Concordance par profil",     Mutualise = 94, Dedie = 93, DedieHautDeGamme = 96 },
		};
	}

	private double Etp(string profil, Dictionary<string, string> profilToSeg)
	{
		var pf = Regles.PortefeuillesTheoriques.FirstOrDefault(p => p.Profil == profil);
		if (pf == null || pf.ClientsParConseiller <= 0) return 0;
		var seg = profilToSeg.GetValueOrDefault(profil, "");
		return (double)ClientsFixe.GetValueOrDefault(seg, 0) / pf.ClientsParConseiller;
	}
}
