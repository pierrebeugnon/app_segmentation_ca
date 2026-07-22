using Segmentation.Client.Models;
using Segmentation.Shared.Models;

namespace Segmentation.Client.Services
{
    /// <summary>
    /// Service de calcul du dashboard de pilotage.
    /// Consomme SegmentationDistributive + Règles pour produire les KPIs.
    /// 100% calculé à la volée — pas de persistance.
    /// </summary>
    public class DashboardService
    {
        private readonly EtpConversionService _etpService;

        public DashboardService(EtpConversionService etpService)
        {
            _etpService = etpService;
        }

        // ═══════════════════════════════════════════════════════════
        //   KPIs GLOBAUX
        // ═══════════════════════════════════════════════════════════
        public DashboardKpis ComputeGlobalKpis(
            List<SegmentationDistributiveData> data,
            ReglesHypothesesModel? regles)
        {
            if (data == null || !data.Any())
                return new DashboardKpis();

            // 1. Conseillers existants (matricules distincts en poste)
            var conseillersExistants = data
                .Where(x => !string.IsNullOrWhiteSpace(x.MatriculeConseiller))
                .Select(x => x.MatriculeConseiller)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            // 2. Clients commerciaux (somme de tous les segments)
            var clientsCommerciaux =
                data.Sum(x => x.HDGPremiumPotentiel + x.HDGPremiumStandard
                            + x.HDGPotentiel + x.HDGSeniorEpargnant + x.HDGStandard
                            + x.CIPotentiel + x.CIStandard
                            + x.GPPotentiel + x.GPStandard
                            + x.NonSegmente + x.NonClasse);

            // 3. ETP nécessaires (somme des charges théoriques par segment)
            var etpNecessaires = ComputeTotalEtpBesoin(data);

            // 4. Cible théorique de conseillers = arrondi de l'ETP nécessaire
            var conseillersCibles = (int)Math.Round(etpNecessaires);

            // 5. Taux de couverture (référence = cible théorique)
            var tauxCouverture = conseillersCibles > 0
                ? (double)conseillersExistants / conseillersCibles * 100.0
                : 0;

            // 6. Concordance moyenne
            var concordance = ComputeConcordanceMoyenne(data, regles);

            // 7. Taux de rotation portefeuille (placeholder V1)
            var tauxRotationPortefeuille = 12.0;

            return new DashboardKpis
            {
                ConseillersExistants     = conseillersExistants,
                ConseillersCibles        = conseillersCibles,
                ClientsCommerciaux       = clientsCommerciaux,
                EtpNecessaires           = Math.Round(etpNecessaires, 1),
                TauxCouverture           = Math.Round(tauxCouverture, 0),
                Concordance              = Math.Round(concordance, 0),
                TauxRotationPortefeuille = tauxRotationPortefeuille
            };
        }

        // ═══════════════════════════════════════════════════════════
        //   HEATMAP TERRITORIALE
        //   Adaptative selon le niveau filtré :
        //   - Aucun filtre → grille par région
        //   - Filtre région → grille par secteur
        //   - Filtre secteur → grille par agence
        // ═══════════════════════════════════════════════════════════
        public List<TerritoireRow> ComputeHeatmap(
            List<SegmentationDistributiveData> data,
            ReglesHypothesesModel? regles,
            string? filtreRegion,
            string? filtreSecteur)
        {
            if (data == null || !data.Any())
                return new List<TerritoireRow>();

            IEnumerable<IGrouping<string, SegmentationDistributiveData>> groupes;

            if (!string.IsNullOrWhiteSpace(filtreSecteur))
            {
                groupes = data.GroupBy(x => x.LibAgence ?? "");
            }
            else if (!string.IsNullOrWhiteSpace(filtreRegion))
            {
                groupes = data.GroupBy(x => x.LibSecteur ?? "");
            }
            else
            {
                groupes = data.GroupBy(x => x.LibRegion ?? "");
            }

            return groupes
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .Select(g =>
                {
                    var gData = g.ToList();
                    var kpis = ComputeGlobalKpis(gData, regles);

                    return new TerritoireRow
                    {
                        Nom               = g.Key,
                        ClientsGeres      = kpis.ClientsCommerciaux,
                        ConseillersActifs = kpis.ConseillersExistants,
                        EtpNecessaires    = kpis.EtpNecessaires,
                        TauxCouverture    = kpis.TauxCouverture,
                        Concordance       = kpis.Concordance
                    };
                })
                .OrderByDescending(x => x.ClientsGeres)
                .ToList();
        }

        // ═══════════════════════════════════════════════════════════
        //   RÉPARTITION ETP PAR SEGMENT (pour graphique)
        // ═══════════════════════════════════════════════════════════
        public List<SegmentRepartition> ComputeRepartitionParSegment(
            List<SegmentationDistributiveData> data)
        {
            if (data == null || !data.Any())
                return new List<SegmentRepartition>();

            var segments = new[]
            {
                ("HDG Premium Potentiel", data.Sum(x => x.HDGPremiumPotentiel)),
                ("HDG Premium Standard",  data.Sum(x => x.HDGPremiumStandard)),
                ("HDG Potentiel",         data.Sum(x => x.HDGPotentiel)),
                ("HDG Senior Epargnant",  data.Sum(x => x.HDGSeniorEpargnant)),
                ("HDG Standard",          data.Sum(x => x.HDGStandard)),
                ("CI Potentiel",          data.Sum(x => x.CIPotentiel)),
                ("CI Standard",           data.Sum(x => x.CIStandard)),
                ("GP Potentiel",          data.Sum(x => x.GPPotentiel)),
                ("GP Standard",           data.Sum(x => x.GPStandard))
            };

            var total = segments.Sum(s => s.Item2);
            if (total == 0) return new List<SegmentRepartition>();

            return segments
                .Where(s => s.Item2 > 0)
                .Select(s => new SegmentRepartition
                {
                    Segment = s.Item1,
                    NbClients = s.Item2,
                    Pourcentage = Math.Round((double)s.Item2 / total * 100, 1)
                })
                .OrderByDescending(x => x.NbClients)
                .ToList();
        }

        // ═══════════════════════════════════════════════════════════
        //   TOP/FLOP AGENCES — Par concordance croissante (flop = alerte)
        // ═══════════════════════════════════════════════════════════
        public List<TerritoireRow> ComputeAgencesEnAlerte(
            List<SegmentationDistributiveData> data,
            ReglesHypothesesModel? regles,
            int top = 5)
        {
            if (data == null || !data.Any())
                return new List<TerritoireRow>();

            return data
                .GroupBy(x => new { x.LibAgence, x.LibSecteur, x.LibRegion })
                .Where(g => !string.IsNullOrWhiteSpace(g.Key.LibAgence))
                .Select(g =>
                {
                    var gData = g.ToList();
                    var kpis = ComputeGlobalKpis(gData, regles);

                    return new TerritoireRow
                    {
                        Nom               = g.Key.LibAgence,
                        Secteur           = g.Key.LibSecteur,
                        Region            = g.Key.LibRegion,
                        ClientsGeres      = kpis.ClientsCommerciaux,
                        ConseillersActifs = kpis.ConseillersExistants,
                        EtpNecessaires    = kpis.EtpNecessaires,
                        TauxCouverture    = kpis.TauxCouverture,
                        Concordance       = kpis.Concordance
                    };
                })
                .OrderBy(x => x.Concordance)
                .Take(top)
                .ToList();
        }

        // ═══════════════════════════════════════════════════════════
        //   RÉPARTITION CONSEILLERS × TAILLE THÉORIQUE PORTEFEUILLE
        //   Par ligne métier (Banque Privée / Retail) + détail profils
        // ═══════════════════════════════════════════════════════════
        public List<LigneMetierRepartition> ComputeRepartitionConseillers(
            List<SegmentationDistributiveData> data,
            ReglesHypothesesModel? regles)
        {
            if (data == null || !data.Any())
                return new List<LigneMetierRepartition>();

            // Segments BP (contiennent "Premium")
            var segmentsBP = new[] { "HDG Premium Potentiel", "HDG Premium Standard" };

            // Segments Retail (autres segments commerciaux)
            var segmentsRetail = new[]
            {
                "HDG Potentiel", "HDG Senior Epargnant", "HDG Standard",
                "CI Potentiel", "CI Standard",
                "GP Potentiel", "GP Standard"
            };

            // Profils par ligne métier depuis le référentiel R&H
            var profilsBP = regles?.ConseillersProfils
                .Where(p => string.Equals(p.LigneMetier, "banque privée", StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Profil?.Trim() ?? "")
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<string>();

            var profilsRetail = regles?.ConseillersProfils
                .Where(p => string.Equals(p.LigneMetier, "retail", StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Profil?.Trim() ?? "")
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<string>();

            // Compte les conseillers existants (matricules distincts) par profil
            int CountExistantByProfil(string profil) =>
                data.Where(x => !string.IsNullOrWhiteSpace(x.MatriculeConseiller))
                    .Where(x => string.Equals(x.TypeConseiller?.Trim(), profil, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.MatriculeConseiller)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

            var existantsBPByProfil = profilsBP
                .ToDictionary(p => p, p => CountExistantByProfil(p), StringComparer.OrdinalIgnoreCase);

            var existantsRetailByProfil = profilsRetail
                .ToDictionary(p => p, p => CountExistantByProfil(p), StringComparer.OrdinalIgnoreCase);

            var conseillersExistantsBP     = existantsBPByProfil.Values.Sum();
            var conseillersExistantsRetail = existantsRetailByProfil.Values.Sum();

            // Clients par ligne métier
            var clientsBP = data.Sum(x => x.HDGPremiumPotentiel + x.HDGPremiumStandard);
            var clientsRetail = data.Sum(x =>
                x.HDGPotentiel + x.HDGSeniorEpargnant + x.HDGStandard
                + x.CIPotentiel + x.CIStandard
                + x.GPPotentiel + x.GPStandard);

            // Cible en conseillers = ETP nécessaire arrondi (par ligne métier)
            var etpBesoinBP = segmentsBP
                .Select(seg => _etpService.GetChargeTotaleEtpForSegment(data, seg) ?? 0)
                .Sum();

            var etpBesoinRetail = segmentsRetail
                .Select(seg => _etpService.GetChargeTotaleEtpForSegment(data, seg) ?? 0)
                .Sum();

            var conseillersCiblesBP     = (int)Math.Round(etpBesoinBP);
            var conseillersCiblesRetail = (int)Math.Round(etpBesoinRetail);

            // Taille théorique portefeuille = clients / nb conseillers
            var taillePfExistBP = conseillersExistantsBP > 0
                ? (double)clientsBP / conseillersExistantsBP : 0;
            var taillePfCibleBP = conseillersCiblesBP > 0
                ? (double)clientsBP / conseillersCiblesBP : 0;

            var taillePfExistRetail = conseillersExistantsRetail > 0
                ? (double)clientsRetail / conseillersExistantsRetail : 0;
            var taillePfCibleRetail = conseillersCiblesRetail > 0
                ? (double)clientsRetail / conseillersCiblesRetail : 0;

            // Répartition proportionnelle des cibles entre profils
            List<ProfilConseillerRepartition> BuildProfilDetail(
                List<string> profils,
                Dictionary<string, int> existantsByProfil,
                int totalExistant,
                int totalCible)
            {
                return profils
                    .OrderBy(p => p)
                    .Select(p =>
                    {
                        var existant = existantsByProfil.GetValueOrDefault(p);

                        // Cible par profil = proportionnelle à l'existant
                        // Fallback : répartition uniforme si existant = 0
                        var cible = totalExistant > 0
                            ? (int)Math.Round((double)existant * totalCible / totalExistant)
                            : (profils.Count > 0 ? totalCible / profils.Count : 0);

                        return new ProfilConseillerRepartition
                        {
                            Profil = p,
                            ConseillersExistants = existant,
                            ConseillersCibles = cible
                        };
                    })
                    .ToList();
            }

            return new List<LigneMetierRepartition>
            {
                new()
                {
                    LigneMetier = "Banque Privée",
                    NbClients = clientsBP,
                    ConseillersExistants = conseillersExistantsBP,
                    ConseillersCibles = conseillersCiblesBP,
                    TaillePortefeuilleExistant = Math.Round(taillePfExistBP, 0),
                    TaillePortefeuilleCible = Math.Round(taillePfCibleBP, 0),
                    Profils = BuildProfilDetail(
                        profilsBP, existantsBPByProfil,
                        conseillersExistantsBP, conseillersCiblesBP)
                },
                new()
                {
                    LigneMetier = "Retail",
                    NbClients = clientsRetail,
                    ConseillersExistants = conseillersExistantsRetail,
                    ConseillersCibles = conseillersCiblesRetail,
                    TaillePortefeuilleExistant = Math.Round(taillePfExistRetail, 0),
                    TaillePortefeuilleCible = Math.Round(taillePfCibleRetail, 0),
                    Profils = BuildProfilDetail(
                        profilsRetail, existantsRetailByProfil,
                        conseillersExistantsRetail, conseillersCiblesRetail)
                }
            };
        }

        // ═══════════════════════════════════════════════════════════
        //   HELPERS PRIVÉS
        // ═══════════════════════════════════════════════════════════

        private double ComputeTotalEtpBesoin(List<SegmentationDistributiveData> data)
        {
            var segments = new[]
            {
                "HDG Premium Potentiel", "HDG Premium Standard",
                "HDG Potentiel", "HDG Senior Epargnant", "HDG Standard",
                "CI Potentiel", "CI Standard",
                "GP Potentiel", "GP Standard"
            };

            var totalEtp = 0.0;
            foreach (var seg in segments)
            {
                var charge = _etpService.GetChargeTotaleEtpForSegment(data, seg);
                if (charge.HasValue) totalEtp += charge.Value;
            }

            return totalEtp;
        }

        private double ComputeConcordanceMoyenne(
            List<SegmentationDistributiveData> data,
            ReglesHypothesesModel? regles)
        {
            if (regles == null || data == null || !data.Any())
                return 0;

            var totalConseillers = data
                .Where(x => !string.IsNullOrWhiteSpace(x.TypeConseiller))
                .Select(x => x.MatriculeConseiller)
                .Distinct()
                .Count();

            if (totalConseillers == 0) return 0;

            var typesPrio = regles.ReglesAffectationSegments
                .Where(r => !string.IsNullOrWhiteSpace(r.ConseillerPrioritaire))
                .Select(r => r.ConseillerPrioritaire!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var conseillersConcordants = data
                .Where(x => !string.IsNullOrWhiteSpace(x.MatriculeConseiller))
                .Where(x => !string.IsNullOrWhiteSpace(x.TypeConseiller))
                .Where(x => typesPrio.Contains(x.TypeConseiller.Trim()))
                .Select(x => x.MatriculeConseiller)
                .Distinct()
                .Count();

            return (double)conseillersConcordants / totalConseillers * 100.0;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //   Modèles de retour
    // ═══════════════════════════════════════════════════════════════
    public class DashboardKpis
    {
        // ── Conseillers ────────────────────────────
        public int ConseillersExistants { get; set; }
        public int ConseillersCibles { get; set; }

        // ── Clients ────────────────────────────────
        public int ClientsCommerciaux { get; set; }

        // ── Indicateurs de performance ─────────────
        public double EtpNecessaires { get; set; }
        public double TauxCouverture { get; set; }
        public double Concordance { get; set; }
        public double TauxRotationPortefeuille { get; set; }
    }

    public class TerritoireRow
    {
        public string Nom { get; set; } = "";
        public string? Secteur { get; set; }
        public string? Region { get; set; }
        public int ClientsGeres { get; set; }
        public int ConseillersActifs { get; set; }
        public double EtpNecessaires { get; set; }
        public double TauxCouverture { get; set; }
        public double Concordance { get; set; }
    }

    public class SegmentRepartition
    {
        public string Segment { get; set; } = "";
        public int NbClients { get; set; }
        public double Pourcentage { get; set; }
    }

    public class LigneMetierRepartition
    {
        public string LigneMetier { get; set; } = "";
        public int NbClients { get; set; }

        public int ConseillersExistants { get; set; }
        public int ConseillersCibles { get; set; }

        public double TaillePortefeuilleExistant { get; set; }
        public double TaillePortefeuilleCible { get; set; }

        // Détail par type de conseiller (issu du référentiel R&H)
        public List<ProfilConseillerRepartition> Profils { get; set; } = new();

        public int DeltaConseillers => ConseillersExistants - ConseillersCibles;
        public double DeltaTaillePortefeuille => TaillePortefeuilleExistant - TaillePortefeuilleCible;
    }

    public class ProfilConseillerRepartition
    {
        public string Profil { get; set; } = "";
        public int ConseillersExistants { get; set; }
        public int ConseillersCibles { get; set; }
        public int DeltaConseillers => ConseillersExistants - ConseillersCibles;
    }
}
