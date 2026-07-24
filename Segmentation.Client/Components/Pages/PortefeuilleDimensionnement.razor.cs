using Microsoft.AspNetCore.Components;
using MudBlazor;
using Segmentation.Client.Components.Dialogs;
using Segmentation.Client.Models;
using Segmentation.Client.Services;
using Segmentation.Shared.Models;
using System.Net.Http.Json;

namespace Segmentation.Client.Components.Pages;

/// <summary>Logique et état de la page Dimensionnement des Portefeuilles.</summary>
public partial class PortefeuilleDimensionnement : ComponentBase, IDisposable
{
	[Inject] private SegmentationStateService StateService { get; set; } = default!;
	[Inject] private IDialogService DialogService { get; set; } = default!;
	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] private RepartitionAutomatiqueService RepartitionService { get; set; } = default!;
	[Inject] private HttpClient Http { get; set; } = default!;
	private bool vueEtp = true;
	private bool _isLoading = false;
	private List<string> profils = new();   // types distincts (lookup assignment)
	private List<string> segments = new();
	private List<DimMatrixRow> rows = new();
	private List<ConseillerSlot> _slots = new();
	private List<SegmentationDistributiveData> _segmentationDistributive = new();
	private List<SegmentationDistributiveData> _segmentationDistributiveFiltree = new();

	// ── Vue détail / consolidée par métier ────────────────────────────────
	// Métiers présents dans ce set → colonne agrégée (repliée)
	private HashSet<string> _collapsedProfils = new();

	private void ToggleProfil(string profil)
	{
		if (!_collapsedProfils.Remove(profil))
			_collapsedProfils.Add(profil);
	}

	// Ordre d'affichage des conseillers d'un métier dans le tableau 2 (inversé)
	private IEnumerable<ConseillerSlot> SlotsForProfil(string profil) =>
		_slots.Where(s => s.Profil.Equals(profil, StringComparison.OrdinalIgnoreCase)).Reverse();

	// Nombre total de colonnes "slots" visibles (pour les cellules colspan/count)
	private int VisibleSlotColumnCount =>
		profils.Sum(p => _collapsedProfils.Contains(p)
			? 1
			: _slots.Count(s => s.Profil.Equals(p, StringComparison.OrdinalIgnoreCase)));

	// ETP existant agrégé pour un profil sur une ligne
	private double? GetAggregateExistantForProfil(DimMatrixRow row, string profil)
	{
		var vals = _slots.Where(s => s.Profil.Equals(profil, StringComparison.OrdinalIgnoreCase))
						 .Select(s => row.EtpParProfil.GetValueOrDefault(s.Id))
						 .Where(v => v.HasValue).ToList();
		return vals.Any() ? vals.Sum(v => v!.Value) : null;
	}

	// ETP cible saisi agrégé pour un profil sur une ligne
	private double? GetAggregateCibleForProfil(DimMatrixRow row, string profil)
	{
		var vals = _slots.Where(s => s.Profil.Equals(profil, StringComparison.OrdinalIgnoreCase) && s.IsCible)
						 .Select(s => row.EtpCibleSaisieParProfil.GetValueOrDefault(s.Id))
						 .Where(v => v.HasValue).ToList();
		return vals.Any() ? vals.Sum(v => v!.Value) : null;
	}

	// Code couleur du taux de remplissage (% de la capacité effectivement chargée)
	// ~100% = vert (bien rempli) · < 100% = rouge (sous-chargé) · > 100% = orange (surchargé)
	private static string RemplissageColor(double? pct) =>
		!pct.HasValue ? "color:white" :
		Math.Abs(pct.Value - 100) < 1 ? "color:#A5D6A7;font-weight:700" :
		pct.Value < 100 ? "color:#EF9A9A;font-weight:700" :
								  "color:#FFD54F;font-weight:700";

	// KPI agrégés par profil : taux de remplissage CIBLE (charge cible ÷ capacité cible × 100)
	private double? GetRemplissageForProfil(string profil)
	{
		var slotsOfProfil = _slots.Where(s => s.Profil.Equals(profil, StringComparison.OrdinalIgnoreCase) && s.IsCible).ToList();
		var totalCap = slotsOfProfil.Sum(s => s.EtpCible);
		if (totalCap <= 0) return null;
		var totalCharge = slotsOfProfil.Sum(s => GetEtpCibleSaisieTotal(s.Id) ?? 0);
		return totalCharge / totalCap * 100.0;
	}

	private double? GetRemplissageTotal()
	{
		var slotsCible = _slots.Where(s => s.IsCible).ToList();
		var totalCap = slotsCible.Sum(s => s.EtpCible);
		if (totalCap <= 0) return null;
		var totalCharge = slotsCible.Sum(s => GetEtpCibleSaisieTotal(s.Id) ?? 0);
		return totalCharge / totalCap * 100.0;
	}

	// Taux de remplissage EXISTANT (pour comparaison) : charge existante ÷ effectif existant × 100
	private double? GetRemplissageExistantForProfil(string profil)
	{
		var slotsOfProfil = _slots.Where(s => s.Profil.Equals(profil, StringComparison.OrdinalIgnoreCase) && s.IsActuel).ToList();
		var totalCap = slotsOfProfil.Sum(s => s.EtpActuel);
		if (totalCap <= 0) return null;
		var totalCharge = slotsOfProfil.Sum(s => GetTotalEtpExistantForSlot(s.Id) ?? 0);
		return totalCharge / totalCap * 100.0;
	}

	private double? GetRemplissageExistantTotal()
	{
		var slotsActuel = _slots.Where(s => s.IsActuel).ToList();
		var totalCap = slotsActuel.Sum(s => s.EtpActuel);
		if (totalCap <= 0) return null;
		var totalCharge = slotsActuel.Sum(s => GetTotalEtpExistantForSlot(s.Id) ?? 0);
		return totalCharge / totalCap * 100.0;
	}

	private double GetEvolutionForProfil(string profil) =>
		_slots.Where(s => s.Profil.Equals(profil, StringComparison.OrdinalIgnoreCase))
			  .Sum(s => GetEvolutionForSlot(s));

	private double? GetEtpCibleSaisieTotalForProfil(string profil)
	{
		var vals = _slots.Where(s => s.Profil.Equals(profil, StringComparison.OrdinalIgnoreCase))
						 .Select(s => GetEtpCibleSaisieTotal(s.Id))
						 .Where(v => v.HasValue).ToList();
		return vals.Any() ? vals.Sum(v => v!.Value) : null;
	}

	// ── KPI — Indicateurs agrégés par profil ──────────────────────────────
	private Dictionary<string, double?> _tauxRotation = new();
	private Dictionary<string, double?> _tauxEntree = new();

	// ── Filtres ───────────────────────────────────────────────────────────
	private string? filtreRegion = null;
	private string? filtreSecteur = null;
	private string? filtreAgence = null;

	private string? filtreRegionApplique = null;
	private string? filtreSecteurApplique = null;
	private string? filtreAgenceApplique = null;

	private List<string> allRegions = new();
	private List<string> allSecteurs = new();
	private List<string> allAgences = new();

	private List<DimMatrixRow> filteredRows = new();

	// ── Labels courts par profil ──────────────────────────────────────────
	private static readonly Dictionary<string, string> _profilShort =
		new(StringComparer.OrdinalIgnoreCase)
		{
			["DIR. BP"] = "DIR",
			["BANQUIER PRIVÉ"] = "BP",
			["CGP"] = "CGP",
			["RESP. AGENCE"] = "RA",
			["RCP"] = "RCP",
			["CONSEILLER CLIENTELE"] = "CC",
			["CONSEILLER COMMERCIAL"] = "COM",
		};

	// ── Lifecycle ─────────────────────────────────────────────────────────
	protected override async Task OnInitializedAsync()
	{
		StateService.OnChange += OnReglesChanged;
		await LoadDataAsync();
		InitData();
	}

	private async Task LoadDataAsync()
	{
		_isLoading = true;
		try
		{
			_segmentationDistributive = await Http.GetFromJsonAsync<List<SegmentationDistributiveData>>(
				"api/SegmentationDistributive") ?? new();
			_segmentationDistributiveFiltree = _segmentationDistributive.ToList();
			Console.WriteLine($">>> SegmentationDistributive chargée : {_segmentationDistributive.Count} lignes");
		}
		catch (Exception ex)
		{
			Console.WriteLine($">>> Erreur chargement SegmentationDistributive : {ex.Message}");
		}
		finally
		{
			_isLoading = false;
		}
	}

	public void Dispose() => StateService.OnChange -= OnReglesChanged;

	private void OnReglesChanged()
	{
		profils = GetProfils();
		segments = GetSegments();
		// Ajouter des slots si de nouveaux profils apparaissent
		foreach (var p in profils.Where(p => !_slots.Any(s => s.Profil.Equals(p, StringComparison.OrdinalIgnoreCase))))
		{
			var prefix = _profilShort.GetValueOrDefault(p, p[..Math.Min(3, p.Length)].ToUpper());
			var newSlot = new ConseillerSlot { Profil = p, Label = prefix + "-1", IsActuel = true, IsCible = true };
			_slots.Add(newSlot);
			foreach (var row in rows) { row.EtpParProfil[newSlot.Id] = null; row.EtpCibleSaisieParProfil[newSlot.Id] = null; }
		}
		BuildFilterOptions();
		RecalcTotal();
		InvokeAsync(StateHasChanged);
	}

	private void InitData()
	{
		profils = GetProfils();
		segments = GetSegments();
		_slots = BuildSlots();
		rows = BuildRows();
		RecalcTotal();
		InitEffectifs();
		BuildFilterOptions();
		ApplyFilters();
	}

	// ── Construction des slots individuels ───────────────────────────────
	private List<ConseillerSlot> BuildSlots()
	{
		// Données réelles si disponibles
		if (_segmentationDistributiveFiltree is { Count: > 0 })
		{
			return _segmentationDistributiveFiltree
				.Where(x => !string.IsNullOrWhiteSpace(x.MatriculeConseiller))
				.Select(x => new ConseillerSlot
				{
					Profil = x.TypeConseiller,
					Label = $"{_profilShort.GetValueOrDefault(x.TypeConseiller, x.TypeConseiller[..Math.Min(3, x.TypeConseiller.Length)].ToUpper())}-{x.MatriculeConseiller}",
					EtpActuel = x.Etp ?? 0,
					EtpCible = x.Etp ?? 0,
					IsActuel = true,
					IsCible = true
				})
				.GroupBy(s => s.Label)
				.Select(g => g.First())
				.Reverse()
				.ToList();
		}

		// Fallback mock
		var result = new List<ConseillerSlot>();
		foreach (var profil in GetProfils())
		{
			var count = (int)(_mockEffectifActuel.TryGetValue(profil, out var cnt) ? cnt : 1);
			var prefix = _profilShort.GetValueOrDefault(profil, profil[..Math.Min(3, profil.Length)].ToUpper());
			for (int i = 1; i <= count; i++)
			{
				result.Add(new ConseillerSlot
				{
					Profil = profil,
					Label = count > 1 ? $"{prefix}-{i}" : prefix,
					IsActuel = true,
					IsCible = true
				});
			}
		}
		return result;
	}

	// ── Gestion des slots ─────────────────────────────────────────────────
	private void AddSlot(string profilType)
	{
		var prefix = _profilShort.GetValueOrDefault(profilType, profilType[..Math.Min(3, profilType.Length)].ToUpper());
		var num = _slots.Count(s => s.Profil.Equals(profilType, StringComparison.OrdinalIgnoreCase)) + 1;
		var slot = new ConseillerSlot { Profil = profilType, Label = $"{prefix}-{num}", IsActuel = false, IsCible = true };
		_slots.Add(slot);
		foreach (var row in rows)
		{
			row.EtpParProfil[slot.Id] = null;
			row.EtpCibleSaisieParProfil[slot.Id] = null;
		}
		RecalcTotal();
	}

	// Suppression définitive d'un slot "Nouveau" (IsActuel=false)
	private void RemoveNewSlot(ConseillerSlot slot)
	{
		_slots.Remove(slot);
		foreach (var row in rows)
		{
			row.EtpParProfil.Remove(slot.Id);
			row.EtpCibleSaisieParProfil.Remove(slot.Id);
		}
		RecalcTotal();
	}

	// Retire un effectif EXISTANT de la répartition cible (il reste dans l'existant, grisé).
	// La modale de redistribution ne se déclenche que si le conseiller porte une charge sur
	// la CIBLE (EtpCibleSaisieParProfil) — l'existant (EtpParProfil) n'entre jamais en compte
	// dans cette décision, puisqu'on ne fait que le retirer de la répartition cible.
	private async Task RemoveCibleForExistingSlot(ConseillerSlot slot)
	{
		var chargeRows = rows.Where(r => !r.IsTotal && (r.EtpCibleSaisieParProfil.GetValueOrDefault(slot.Id) ?? 0) > 0.001).ToList();

		if (chargeRows.Any())
		{
			var autreConseillerExiste = _slots.Any(s => s.Id != slot.Id && s.IsCible);
			if (!autreConseillerExiste)
			{
				var totalCharge = chargeRows.Sum(r => r.EtpCibleSaisieParProfil.GetValueOrDefault(slot.Id) ?? 0);
				Snackbar.Add(
					$"Impossible de retirer {slot.Label} de la cible : {totalCharge:F2} ETP sont encore répartis sur {chargeRows.Count} segment(s) " +
					"et aucun autre conseiller n'est disponible pour reprendre cette charge.",
					Severity.Warning);
				return;
			}

			var parameters = new DialogParameters<RedistribuerChargeDialog>
			{
				{ x => x.SlotToRemove, slot },
				{ x => x.ChargeRows, chargeRows },
				{ x => x.AllRows, rows },
				{ x => x.AllSlots, _slots },
				{ x => x.Regles, StateService.Regles.ReglesAffectationSegments },
			};
			var dialogRef = await DialogService.ShowAsync<RedistribuerChargeDialog>(
				$"Répartir la charge cible de {slot.Label} avant suppression",
				parameters,
				new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true });
			var result = await dialogRef.Result;
			if (result is null || result.Canceled) return;

			// Garde-fou : si un reliquat de charge subsiste malgré tout, on bloque le retrait
			var reliquat = rows.Where(r => !r.IsTotal).Sum(r => r.EtpCibleSaisieParProfil.GetValueOrDefault(slot.Id) ?? 0);
			if (reliquat > 0.001)
			{
				Snackbar.Add($"{reliquat:F2} ETP n'ont pas pu être réattribués : {slot.Label} reste dans la répartition cible.", Severity.Warning);
				return;
			}
		}

		slot.IsCible = false;
		RecalcTotal();
	}

	// Réintègre un effectif existant précédemment retiré de la cible
	private void RestoreCible(ConseillerSlot slot)
	{
		slot.IsCible = true;
		RecalcTotal();
	}

	// ── Filtres ───────────────────────────────────────────────────────────
	private void BuildFilterOptions()
	{
		var si = StateService.Regles.SegmentsIntensite;
		allRegions = si.Select(s => s.Region).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
		allSecteurs = si.Select(s => s.Secteur).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
		allAgences = si.Select(s => s.Agence).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
	}

	private void ApplyFilters()
	{
		var si = StateService.Regles.SegmentsIntensite;
		var segsFiltres = si
			.Where(s => (string.IsNullOrEmpty(filtreRegion) || s.Region == filtreRegion)
					 && (string.IsNullOrEmpty(filtreSecteur) || s.Secteur == filtreSecteur)
					 && (string.IsNullOrEmpty(filtreAgence) || s.Agence == filtreAgence))
			.Select(s => s.Segment)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		filteredRows = segsFiltres.Any()
			? rows.Where(r => r.IsTotal || segsFiltres.Contains(r.Segment)).ToList()
			: rows.ToList();

		filtreRegionApplique = filtreRegion;
		filtreSecteurApplique = filtreSecteur;
		filtreAgenceApplique = filtreAgence;
	}

	private void ClearFilters()
	{
		filtreRegion = filtreAgence = filtreSecteur = null;
		filtreRegionApplique = filtreSecteurApplique = filtreAgenceApplique = null;
		ApplyFilters();
	}

	private void OnRegionChanged(string? newRegion)
	{
		filtreRegion = newRegion;
		filtreSecteur = null;
		filtreAgence = null;
		BuildFilterOptions();
	}

	private void OnSecteurChanged(string? newSecteur)
	{
		filtreSecteur = newSecteur;
		filtreAgence = null;
		BuildFilterOptions();
	}

	private void OnAgenceChanged(string? newAgence)
	{
		filtreAgence = newAgence;
	}

	private bool HasPendingFilterChanges =>
		filtreRegion != filtreRegionApplique ||
		filtreSecteur != filtreSecteurApplique ||
		filtreAgence != filtreAgenceApplique;

	// ── Sources de données ────────────────────────────────────────────────
	// Ordre d'affichage inversé : du conseiller commercial (COM) au directeur (DIR),
	// utilisé pour les deux tableaux (Effectifs et Répartition des charges)
	private List<string> GetProfils() =>
		StateService.Regles.ConseillersProfils
			.Select(p => p.Profil)
			.Where(p => !string.IsNullOrWhiteSpace(p))
			.Distinct().Reverse().ToList();

	private List<string> GetSegments() =>
		StateService.Regles.SegmentsIntensite
			.Select(s => s.Segment)
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.Distinct().ToList();

	// ── Construction de la table (keyed by slot.Id) ───────────────────────

	// Mock ETP existants par segment × profil
	private static readonly Dictionary<string, Dictionary<string, double>> _mockEtpExistant =
		new(StringComparer.OrdinalIgnoreCase)
		{
			["HDG Premium Potentiel"] = new(StringComparer.OrdinalIgnoreCase) { ["BANQUIER PRIVÉ"] = 0.30, ["DIR. BP"] = 0.10 },
			["HDG Premium Standard"] = new(StringComparer.OrdinalIgnoreCase) { ["CGP"] = 0.85, ["BANQUIER PRIVÉ"] = 0.20 },
			["HDG Potentiel"] = new(StringComparer.OrdinalIgnoreCase) { ["RESP. AGENCE"] = 1.20, ["RCP"] = 0.35 },
			["HDG Senior Epargnant"] = new(StringComparer.OrdinalIgnoreCase) { ["RCP"] = 1.50, ["RESP. AGENCE"] = 0.40 },
			["HDG Standard"] = new(StringComparer.OrdinalIgnoreCase) { ["RESP. AGENCE"] = 2.10, ["CONSEILLER CLIENTELE"] = 0.80 },
			["CI Potentiel"] = new(StringComparer.OrdinalIgnoreCase) { ["CONSEILLER CLIENTELE"] = 3.60, ["RCP"] = 0.50 },
			["CI Standard"] = new(StringComparer.OrdinalIgnoreCase) { ["CONSEILLER CLIENTELE"] = 7.80, ["CONSEILLER COMMERCIAL"] = 2.40 },
			["GP Potentiel"] = new(StringComparer.OrdinalIgnoreCase) { ["CONSEILLER CLIENTELE"] = 3.90 },
			["GP Standard"] = new(StringComparer.OrdinalIgnoreCase) { ["CONSEILLER COMMERCIAL"] = 11.50, ["CONSEILLER CLIENTELE"] = 3.10 },
			["Non segmenté"] = new(StringComparer.OrdinalIgnoreCase) { ["CONSEILLER COMMERCIAL"] = 4.80 },
		};

	// Mock effectifs actuel par profil (nb conseillers en poste)
	private static readonly Dictionary<string, double> _mockEffectifActuel =
		new(StringComparer.OrdinalIgnoreCase)
		{
			["DIR. BP"] = 1,
			["BANQUIER PRIVÉ"] = 3,
			["CGP"] = 2,
			["RESP. AGENCE"] = 2,
			["RCP"] = 4,
			["CONSEILLER CLIENTELE"] = 6,
			["CONSEILLER COMMERCIAL"] = 5,
		};

	private List<DimMatrixRow> BuildRows()
	{
		var dataRows = segments.Select(s => new DimMatrixRow
		{
			Segment = s,
			EtpParProfil = _slots.ToDictionary(sl => sl.Id, sl => GetMockEtpForSlot(s, sl)),
			EtpCibleSaisieParProfil = _slots.ToDictionary(sl => sl.Id, _ => (double?)null)
		}).ToList();

		dataRows.Add(new DimMatrixRow
		{
			Segment = "Charge totale ETP par portefeuille",
			IsTotal = true,
			EtpParProfil = _slots.ToDictionary(sl => sl.Id, _ => (double?)null),
			EtpCibleSaisieParProfil = _slots.ToDictionary(sl => sl.Id, _ => (double?)null)
		});
		return dataRows;
	}

	// ETP existant par slot = total profil / nombre de slots du même profil
	private double? GetMockEtpForSlot(string segment, ConseillerSlot slot)
	{
		if (!_mockEtpExistant.TryGetValue(segment, out var segDict)) return null;
		if (!segDict.TryGetValue(slot.Profil, out var totalEtp)) return null;
		var count = _slots.Count(s => s.Profil.Equals(slot.Profil, StringComparison.OrdinalIgnoreCase));
		return count > 0 ? Math.Round(totalEtp / count, 3) : null;
	}

	// ── Affectation depuis ReglesHypotheses ───────────────────────────────
	private int GetAssignmentLevel(string segment, string profil)
	{
		if (string.IsNullOrWhiteSpace(segment) || string.IsNullOrWhiteSpace(profil)) return 0;
		var r = StateService.Regles.ReglesAffectationSegments
			.FirstOrDefault(x => x.Segment?.Trim().Equals(segment.Trim(), StringComparison.OrdinalIgnoreCase) == true);
		if (r == null) return 0;
		if (r.ConseillerPrioritaire?.Trim().Equals(profil.Trim(), StringComparison.OrdinalIgnoreCase) == true) return 1;
		if (r.ConseillerSecondaire?.Trim().Equals(profil.Trim(), StringComparison.OrdinalIgnoreCase) == true) return 2;
		if (r.ConseillerTertiaire?.Trim().Equals(profil.Trim(), StringComparison.OrdinalIgnoreCase) == true) return 3;
		return 0;
	}

	// Style de fond de la cellule (affectation + grisé si slot retiré de la cible)
	private static string GetMergedCellStyle(int level, bool isTotal, bool isGreyed = false)
	{
		if (isGreyed) return "background:#ECECEC;opacity:0.5";
		if (isTotal) return "";
		return level switch
		{
			1 => "background:#1A6B3C;color:white",
			2 => "background:#52A86B;color:white",
			3 => "background:#C3E6CB;color:#1A4E6B",
			_ => ""
		};
	}

	// ── Distribution automatique R&H ─────────────────────────────────────
	private void AutoDistribuerCible()
	{
		var segmentsIncomplets = RepartitionService.DistribuerAutomatiquement(
			rows, _slots, StateService.Regles.ReglesAffectationSegments);

		RecalcTotal();

		if (segmentsIncomplets == 0)
			Snackbar.Add("Distribution R&H appliquée — toute la charge a été placée sur les conseillers prioritaires.", Severity.Success);
		else
			Snackbar.Add($"Distribution R&H appliquée — {segmentsIncomplets} segment(s) n'ont pas pu être entièrement couverts (capacité insuffisante).", Severity.Warning);
	}

	// ── Mutations ─────────────────────────────────────────────────────────
	private void SetEtpCible(DimMatrixRow row, string slotId, double? value)
	{
		row.EtpCibleSaisieParProfil[slotId] = value;
		RecalcTotal();
	}

	private void RecalcTotal()
	{
		var totalRow = rows.LastOrDefault(r => r.IsTotal);
		if (totalRow == null) return;
		foreach (var slot in _slots)
		{
			var valsExist = rows.Where(r => !r.IsTotal).Select(r => r.EtpParProfil.GetValueOrDefault(slot.Id)).ToList();
			var valsCible = rows.Where(r => !r.IsTotal).Select(r => r.EtpCibleSaisieParProfil.GetValueOrDefault(slot.Id)).ToList();
			totalRow.EtpParProfil[slot.Id] = valsExist.Any(v => v.HasValue) ? valsExist.Sum(v => v ?? 0) : null;
			totalRow.EtpCibleSaisieParProfil[slot.Id] = valsCible.Any(v => v.HasValue) ? valsCible.Sum(v => v ?? 0) : null;
		}
		ApplyFilters();
	}

	// ── ETP CIBLE calculé depuis ReglesHypothèses ─────────────────────────
	private double? GetEtpCible(string segment, string profil)
	{
		var si = StateService.Regles.SegmentsIntensite
			.FirstOrDefault(s => s.Segment?.Trim().Equals(segment.Trim(), StringComparison.OrdinalIgnoreCase) == true);
		if (si == null || si.NombreClients <= 0) return null;
		var intensite = si.IntensiteRelationnelle;
		if (intensite <= 0) return null;
		var cp = StateService.Regles.ConseillersProfils
			.FirstOrDefault(p => p.Profil?.Trim().Equals(profil.Trim(), StringComparison.OrdinalIgnoreCase) == true);
		if (cp == null) return null;
		var heuresAn = StateService.Regles.HeuresTravailParAn;
		var volumeHoraire = heuresAn * cp.PartTempsCommercialPct / 100.0;
		if (volumeHoraire <= 0) return null;
		var tailleTheorique = volumeHoraire / intensite;
		return tailleTheorique > 0 ? Math.Round(si.NombreClients / tailleTheorique, 2) : null;
	}

	// ── Totaux ────────────────────────────────────────────────────────────
	private double? GetEtpCibleSaisieTotal(string slotId)
	{
		var vals = rows.Where(r => !r.IsTotal)
					   .Select(r => r.EtpCibleSaisieParProfil.GetValueOrDefault(slotId))
					   .Where(v => v.HasValue).ToList();
		return vals.Any() ? vals.Sum(v => v!.Value) : null;
	}

	private double? GetRowCibleTotal(DimMatrixRow row)
	{
		var vals = _slots.Select(s => row.EtpCibleSaisieParProfil.GetValueOrDefault(s.Id))
						 .Where(v => v.HasValue).ToList();
		return vals.Any() ? vals.Sum(v => v!.Value) : null;
	}

	private double? GetRowExistantTotal(DimMatrixRow row)
	{
		var vals = _slots.Select(s => row.EtpParProfil.GetValueOrDefault(s.Id))
						 .Where(v => v.HasValue).ToList();
		return vals.Any() ? vals.Sum(v => v!.Value) : null;
	}

	// ── Couverture : différence entre ETP distribué et charge ETP ────────
	private double? GetCoverageDiff(DimMatrixRow row)
	{
		if (row.IsTotal) return null;
		var activeSlots = _slots.Where(s => s.IsCible).ToList();
		var totalCible = activeSlots.Sum(s => row.EtpCibleSaisieParProfil.GetValueOrDefault(s.Id) ?? 0);
		var totalExist = _slots.Sum(s => row.EtpParProfil.GetValueOrDefault(s.Id) ?? 0);
		if (totalCible <= 0 && totalExist <= 0) return null;
		return totalCible - totalExist;
	}

	// ── Concordance : % ETP assigné à un type DÉSIGNÉ dans R&H ───────────
	private (double? pct, string color, string tooltip) GetConcordance(DimMatrixRow row)
	{
		if (row.IsTotal) return (null, "", "");
		var activeSlots = _slots.Where(s => s.IsCible).ToList();
		var totalCible = activeSlots.Sum(s => row.EtpCibleSaisieParProfil.GetValueOrDefault(s.Id) ?? 0);
		if (totalCible <= 0) return (null, "", "Aucune saisie");

		var etpDesigne = activeSlots
			.Where(s => GetAssignmentLevel(row.Segment, s.Profil) > 0)
			.Sum(s => row.EtpCibleSaisieParProfil.GetValueOrDefault(s.Id) ?? 0);
		var etpHorsRH = totalCible - etpDesigne;

		var etpPrio = activeSlots.Where(s => GetAssignmentLevel(row.Segment, s.Profil) == 1).Sum(s => row.EtpCibleSaisieParProfil.GetValueOrDefault(s.Id) ?? 0);
		var etpSec = activeSlots.Where(s => GetAssignmentLevel(row.Segment, s.Profil) == 2).Sum(s => row.EtpCibleSaisieParProfil.GetValueOrDefault(s.Id) ?? 0);
		var etpTert = activeSlots.Where(s => GetAssignmentLevel(row.Segment, s.Profil) == 3).Sum(s => row.EtpCibleSaisieParProfil.GetValueOrDefault(s.Id) ?? 0);

		var pct = etpDesigne / totalCible * 100.0;
		var color = pct >= 99.9 ? "color:#2E7D32;font-weight:700" :
					pct >= 80 ? "color:#E65100;font-weight:700" :
								  "color:#C62828;font-weight:700";

		var tooltip = $"Concordance : {pct:F0}% dans les types R&H\n" +
					  $"  ▸ Prioritaire  : {etpPrio:F2} ETP\n" +
					  $"  ▸ Secondaire   : {etpSec:F2} ETP\n" +
					  $"  ▸ Tertiaire    : {etpTert:F2} ETP\n" +
					  $"  ✗ Hors R&H     : {etpHorsRH:F2} ETP";

		return (pct, color, tooltip);
	}

	// ── KPI : init taux (rotation/entrée keyed par slot.Id, mock null) ────
	private void InitEffectifs()
	{
		foreach (var slot in _slots)
		{
			_tauxRotation.TryAdd(slot.Id, null);
			_tauxEntree.TryAdd(slot.Id, null);
		}
	}

	// Code couleur commun des scores de concordance (%)
	private static string ConcordanceColor(double? pct) =>
		!pct.HasValue ? "color:white" :
		pct.Value >= 99.9 ? "color:#A5D6A7;font-weight:700" :
		pct.Value >= 80 ? "color:#FFD54F;font-weight:700" :
							"color:#EF9A9A;font-weight:700";

	// ── KPI concordance par slot ──────────────────────────────────────────
	private (double? pct, string color) GetConcordanceForSlot(ConseillerSlot slot)
	{
		var totalCible = GetEtpCibleSaisieTotal(slot.Id);
		if (!totalCible.HasValue || totalCible.Value <= 0) return (null, "color:white");

		var etpDesigne = rows.Where(r => !r.IsTotal && GetAssignmentLevel(r.Segment, slot.Profil) > 0)
							 .Sum(r => r.EtpCibleSaisieParProfil.GetValueOrDefault(slot.Id) ?? 0);
		var pct = etpDesigne / totalCible.Value * 100.0;
		return (pct, ConcordanceColor(pct));
	}

	private (double? pct, string color) GetConcordanceForProfil(string profil)
	{
		var slots = _slots.Where(s => s.Profil.Equals(profil, StringComparison.OrdinalIgnoreCase) && s.IsCible).ToList();
		var totalCible = slots.Sum(s => GetEtpCibleSaisieTotal(s.Id) ?? 0);
		if (totalCible <= 0) return (null, "color:white");

		var etpDesigne = slots.Sum(s =>
			rows.Where(r => !r.IsTotal && GetAssignmentLevel(r.Segment, s.Profil) > 0)
				.Sum(r => r.EtpCibleSaisieParProfil.GetValueOrDefault(s.Id) ?? 0));
		var pct = etpDesigne / totalCible * 100.0;
		return (pct, ConcordanceColor(pct));
	}

	// ── Concordance calculée sur la répartition EXISTANTE (pour comparaison) ──
	private (double? pct, string color) GetConcordanceExistantForSlot(ConseillerSlot slot)
	{
		var totalExist = GetTotalEtpExistantForSlot(slot.Id);
		if (!totalExist.HasValue || totalExist.Value <= 0) return (null, "color:white");

		var etpDesigne = rows.Where(r => !r.IsTotal && GetAssignmentLevel(r.Segment, slot.Profil) > 0)
							 .Sum(r => r.EtpParProfil.GetValueOrDefault(slot.Id) ?? 0);
		var pct = etpDesigne / totalExist.Value * 100.0;
		return (pct, ConcordanceColor(pct));
	}

	private (double? pct, string color) GetConcordanceExistantForProfil(string profil)
	{
		var slots = _slots.Where(s => s.Profil.Equals(profil, StringComparison.OrdinalIgnoreCase)).ToList();
		var totalExist = slots.Sum(s => GetTotalEtpExistantForSlot(s.Id) ?? 0);
		if (totalExist <= 0) return (null, "color:white");

		var etpDesigne = slots.Sum(s =>
			rows.Where(r => !r.IsTotal && GetAssignmentLevel(r.Segment, s.Profil) > 0)
				.Sum(r => r.EtpParProfil.GetValueOrDefault(s.Id) ?? 0));
		var pct = etpDesigne / totalExist * 100.0;
		return (pct, ConcordanceColor(pct));
	}

	// ── KPI taux de rotation par slot ─────────────────────────────────────
	private double? GetTauxRotation(ConseillerSlot slot)
	{
		var cible = Math.Round(GetEtpCibleSaisieTotal(slot.Id) ?? 0, 2);
		if (cible <= 0) return null;
		var exist = Math.Round(GetTotalEtpExistantForSlot(slot.Id) ?? 0, 2);
		var diff = Math.Round(cible - exist, 2);
		return diff / cible * 100.0;
	}

	private double? GetTauxRotationForProfil(string profil)
	{
		var slots = _slots.Where(s => s.Profil.Equals(profil, StringComparison.OrdinalIgnoreCase) && s.IsCible).ToList();
		var totalCible = Math.Round(slots.Sum(s => GetEtpCibleSaisieTotal(s.Id) ?? 0), 2);
		if (totalCible <= 0) return null;
		var totalExist = Math.Round(slots.Sum(s => GetTotalEtpExistantForSlot(s.Id) ?? 0), 2);
		var diff = Math.Round(totalCible - totalExist, 2);
		return diff / totalCible * 100.0;
	}

	// ── KPI entrées dans le portefeuille par slot ─────────────────────────
	private double GetEntreesCell(DimMatrixRow row, string slotId)
	{
		var c = Math.Round(row.EtpCibleSaisieParProfil.GetValueOrDefault(slotId) ?? 0, 2);
		var e = Math.Round(row.EtpParProfil.GetValueOrDefault(slotId) ?? 0, 2);
		return Math.Max(0, c - e);
	}

	private double? GetTauxEntreeSlot(ConseillerSlot slot)
	{
		var cible = Math.Round(GetEtpCibleSaisieTotal(slot.Id) ?? 0, 2);
		if (cible <= 0) return null;
		var entrees = rows.Where(r => !r.IsTotal).Sum(r => GetEntreesCell(r, slot.Id));
		return Math.Round(entrees, 2);
	}

	private double? GetTauxEntreeForProfil(string profil)
	{
		var slots = _slots.Where(s => s.Profil.Equals(profil, StringComparison.OrdinalIgnoreCase) && s.IsCible).ToList();
		var totalCible = Math.Round(slots.Sum(s => GetEtpCibleSaisieTotal(s.Id) ?? 0), 2);
		if (totalCible <= 0) return null;
		var entrees = rows.Where(r => !r.IsTotal).Sum(r => slots.Sum(s => GetEntreesCell(r, s.Id)));
		return Math.Round(entrees, 2);
	}

	private double GetTauxEntreeTotal()
	{
		var activeSlots = _slots.Where(s => s.IsCible).ToList();
		var entrees = rows.Where(r => !r.IsTotal).Sum(r => activeSlots.Sum(s => GetEntreesCell(r, s.Id)));
		return Math.Round(entrees, 2);
	}

	// ── KPI par slot individuel ───────────────────────────────────────────

	// Total ETP existant pour un slot (somme sur tous les segments)
	private double? GetTotalEtpExistantForSlot(string slotId)
	{
		var vals = rows.Where(r => !r.IsTotal)
					   .Select(r => r.EtpParProfil.GetValueOrDefault(slotId))
					   .Where(v => v.HasValue).ToList();
		return vals.Any() ? vals.Sum(v => v!.Value) : null;
	}

	// Taux de remplissage CIBLE par slot, en % = charge ETP cible saisie ÷ EtpCible du conseiller × 100
	private double? GetRemplissageForSlot(ConseillerSlot slot)
	{
		if (!slot.IsCible || slot.EtpCible <= 0) return null;
		var etp = GetEtpCibleSaisieTotal(slot.Id);
		return etp.HasValue ? etp.Value / slot.EtpCible * 100.0 : null;
	}

	// Taux de remplissage EXISTANT par slot (pour comparaison) = charge ETP existante ÷ EtpActuel × 100
	private double? GetRemplissageExistantForSlot(ConseillerSlot slot)
	{
		if (!slot.IsActuel || slot.EtpActuel <= 0) return null;
		var etp = GetTotalEtpExistantForSlot(slot.Id);
		return etp.HasValue ? etp.Value / slot.EtpActuel * 100.0 : null;
	}

	// Evolution par slot : +1 si nouveau planifié, -1 si retiré, 0 si stable
	private static double GetEvolutionForSlot(ConseillerSlot slot) =>
		slot.IsCible ? (slot.IsActuel ? 0.0 : 1.0) : (slot.IsActuel ? -1.0 : 0.0);

	// ── Formatage ─────────────────────────────────────────────────────────
	private static string FormatDiff(double? v, string suffix = "") =>
		v.HasValue ? (v.Value >= 0 ? $"+{v.Value:F2}{suffix}" : $"{v.Value:F2}{suffix}") : "—";

	private static string FormatDiff(double v, string suffix = "") =>
		v >= 0 ? $"+{v:F0}{suffix}" : $"{v:F0}{suffix}";

	private static string DiffColor(double? v, bool positifBon = true) =>
		!v.HasValue ? "color:white" :
		v.Value > 0 ? (positifBon ? "color:#A5D6A7;font-weight:700" : "color:#EF9A9A;font-weight:700") :
		v.Value < 0 ? (positifBon ? "color:#EF9A9A;font-weight:700" : "color:#A5D6A7;font-weight:700") :
		"color:rgba(255,255,255,0.6);font-weight:600";

	private static string DiffColor(double v, bool positifBon = true) =>
		v > 0 ? (positifBon ? "color:#A5D6A7;font-weight:700" : "color:#EF9A9A;font-weight:700") :
		v < 0 ? (positifBon ? "color:#EF9A9A;font-weight:700" : "color:#A5D6A7;font-weight:700") :
		"color:rgba(255,255,255,0.6);font-weight:600";

	// ═══════════════════════════════════════════════════════════════════════
	//  Méthodes ajoutées pour supporter le markup récent (charges manuelles,
	//  totaux KPI dans le tableau, aides)
	// ═══════════════════════════════════════════════════════════════════════

	private bool _showChargesManuelles = false;

	private async Task ShowAide()
	{
		await DialogService.ShowAsync<AidePortefeuilleDialog>(
			"Aide",
			new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true });
	}

	private async Task ShowAideRepartition()
	{
		await DialogService.ShowAsync<AideRepartitionDialog>(
			"Aide — Répartition des charges",
			new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true });
	}

	// ── Charges manuelles par segment ─────────────────────────────────────
	private Dictionary<string, double?> _chargeATransfererBP = new(StringComparer.OrdinalIgnoreCase);
	private Dictionary<string, double?> _chargeRecueBP = new(StringComparer.OrdinalIgnoreCase);
	private Dictionary<string, double?> _chargeATransfererMutualise = new(StringComparer.OrdinalIgnoreCase);

	private static bool IsHdgPremium(string? segment) =>
		!string.IsNullOrWhiteSpace(segment) &&
		segment.Contains("Premium", StringComparison.OrdinalIgnoreCase);

	private static bool IsGpStandard(string? segment) =>
		string.Equals(segment?.Trim(), "GP Standard", StringComparison.OrdinalIgnoreCase);

	private static bool IsSegmentActionnable(string? segment) =>
		!string.IsNullOrWhiteSpace(segment) &&
		!string.Equals(segment.Trim(), "Non segmenté", StringComparison.OrdinalIgnoreCase) &&
		!string.Equals(segment.Trim(), "Non classé", StringComparison.OrdinalIgnoreCase);

	private double? GetChargeTotaleAffichee(DimMatrixRow row)
	{
		if (row.IsTotal) return null;

		var original = GetRowExistantTotal(row) ?? 0;
		var transBP = _chargeATransfererBP.GetValueOrDefault(row.Segment) ?? 0;
		var recueBP = _chargeRecueBP.GetValueOrDefault(row.Segment) ?? 0;
		var transMut = _chargeATransfererMutualise.GetValueOrDefault(row.Segment) ?? 0;

		return (original - transBP) + (recueBP - transMut);
	}

	private double GetTotalATransfererBP() =>
		_chargeATransfererBP.Values.Where(v => v.HasValue).Sum(v => v!.Value);

	private double GetTotalRecueBP() =>
		_chargeRecueBP.Values.Where(v => v.HasValue).Sum(v => v!.Value);

	private double GetTotalATransfererMutualise() =>
		_chargeATransfererMutualise.Values.Where(v => v.HasValue).Sum(v => v!.Value);

	// ── Concordance existante par ligne + couleurs de badge ────────────────
	private (double? pct, string color, string tooltip) GetConcordanceExistant(DimMatrixRow row)
	{
		if (row.IsTotal) return (null, "", "");
		var allSlots = _slots.ToList();
		var totalExist = allSlots.Sum(s => row.EtpParProfil.GetValueOrDefault(s.Id) ?? 0);
		if (totalExist <= 0) return (null, "", "Aucune donnée existante");

		var etpDesigne = allSlots
			.Where(s => GetAssignmentLevel(row.Segment, s.Profil) > 0)
			.Sum(s => row.EtpParProfil.GetValueOrDefault(s.Id) ?? 0);
		var etpHorsRH = totalExist - etpDesigne;

		var etpPrio = allSlots.Where(s => GetAssignmentLevel(row.Segment, s.Profil) == 1).Sum(s => row.EtpParProfil.GetValueOrDefault(s.Id) ?? 0);
		var etpSec = allSlots.Where(s => GetAssignmentLevel(row.Segment, s.Profil) == 2).Sum(s => row.EtpParProfil.GetValueOrDefault(s.Id) ?? 0);
		var etpTert = allSlots.Where(s => GetAssignmentLevel(row.Segment, s.Profil) == 3).Sum(s => row.EtpParProfil.GetValueOrDefault(s.Id) ?? 0);

		var pct = etpDesigne / totalExist * 100.0;
		var color = pct >= 99.9 ? "color:#2E7D32;font-weight:700" :
					pct >= 80 ? "color:#E65100;font-weight:700" :
								  "color:#C62828;font-weight:700";

		var tooltip = $"Concordance existant : {pct:F0}% dans les types R&H\n" +
					  $"  ▸ Prioritaire  : {etpPrio:F2} ETP\n" +
					  $"  ▸ Secondaire   : {etpSec:F2} ETP\n" +
					  $"  ▸ Tertiaire    : {etpTert:F2} ETP\n" +
					  $"  ✗ Hors R&H     : {etpHorsRH:F2} ETP";

		return (pct, color, tooltip);
	}

	private static (string bg, string fg) GetConcordanceColors(double? pct) =>
		!pct.HasValue    ? ("#F5F5F5", "#757575") :
		pct.Value >= 99.9 ? ("#E8F5E9", "#2E7D32") :
		pct.Value >= 80   ? ("#FFF3E0", "#E65100") :
							("#FFEBEE", "#C62828");

	// ── Taux de rotation par ligne ────────────────────────────────────────
	private double? GetTauxRotationForRow(DimMatrixRow row)
	{
		if (row.IsTotal) return null;
		var cible = Math.Round(GetRowCibleTotal(row) ?? 0, 2);
		if (cible <= 0) return null;
		var exist = Math.Round(GetRowExistantTotal(row) ?? 0, 2);
		return Math.Round(cible - exist, 2) / cible * 100.0;
	}

	private double? GetTauxRotationExistant(DimMatrixRow row)
	{
		if (row.IsTotal) return null;
		var cible = Math.Round(GetRowCibleTotal(row) ?? 0, 2);
		var exist = Math.Round(GetRowExistantTotal(row) ?? 0, 2);
		if (exist <= 0) return null;
		return Math.Round(cible - exist, 2) / exist * 100.0;
	}

	// ── Totaux des KPI ─────────────────────────────────────────────────────
	private double? GetTotalConcordance()
	{
		var activeSlots = _slots.Where(s => s.IsCible).ToList();
		var totalCible = rows.Where(r => !r.IsTotal)
			.Sum(r => activeSlots.Sum(s => r.EtpCibleSaisieParProfil.GetValueOrDefault(s.Id) ?? 0));
		if (totalCible <= 0) return null;

		var etpDesigne = rows.Where(r => !r.IsTotal)
			.Sum(r => activeSlots
				.Where(s => GetAssignmentLevel(r.Segment, s.Profil) > 0)
				.Sum(s => r.EtpCibleSaisieParProfil.GetValueOrDefault(s.Id) ?? 0));

		return etpDesigne / totalCible * 100.0;
	}

	private double? GetTotalConcordanceExistant()
	{
		var allSlots = _slots.ToList();
		var totalExist = rows.Where(r => !r.IsTotal)
			.Sum(r => allSlots.Sum(s => r.EtpParProfil.GetValueOrDefault(s.Id) ?? 0));
		if (totalExist <= 0) return null;

		var etpDesigne = rows.Where(r => !r.IsTotal)
			.Sum(r => allSlots
				.Where(s => GetAssignmentLevel(r.Segment, s.Profil) > 0)
				.Sum(s => r.EtpParProfil.GetValueOrDefault(s.Id) ?? 0));

		return etpDesigne / totalExist * 100.0;
	}

	private double? GetTotalTauxRotation()
	{
		var cible = rows.Where(r => !r.IsTotal).Sum(r => GetRowCibleTotal(r) ?? 0);
		if (cible <= 0) return null;
		var exist = rows.Where(r => !r.IsTotal).Sum(r => GetRowExistantTotal(r) ?? 0);
		return Math.Round(cible - exist, 2) / cible * 100.0;
	}

	private double? GetTotalTauxRotationExistant()
	{
		var cible = rows.Where(r => !r.IsTotal).Sum(r => GetRowCibleTotal(r) ?? 0);
		var exist = rows.Where(r => !r.IsTotal).Sum(r => GetRowExistantTotal(r) ?? 0);
		if (exist <= 0) return null;
		return Math.Round(cible - exist, 2) / exist * 100.0;
	}
}
