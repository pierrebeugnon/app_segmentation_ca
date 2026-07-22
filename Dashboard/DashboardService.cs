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

            // 8. Taux de pureté des portefeuilles (existant / cible)
            var tauxPurete = ComputeTauxPurete(data, regles);

            return new DashboardKpis
            {
                ConseillersExistants     = conseillersExistants,
                ConseillersCibles        = conseillersCibles,
                ClientsCommerciaux       = clientsCommerciaux,
                EtpNecessaires           = Math.Round(etpNecessaires, 1),
                TauxCouverture           = Math.Round(tauxCouverture, 0),
                Concordance              = Math.Round(concordance, 0),
                TauxRotationPortefeuille = tauxRotationPortefeuille,
                TauxPurete               = Math.Round(tauxPurete, 0)
            };
        }

        // ═══════════════════════════════════════════════════════════
        //   HEATMAP TERRITORIALE (adaptative — legacy)
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
                        ConseillersCibles = kpis.ConseillersCibles,
                        EtpNecessaires    = kpis.EtpNecessaires,
                        TauxCouverture    = kpis.TauxCouverture,
                        Concordance       = kpis.Concordance,
                        TauxRotation      = kpis.TauxRotationPortefeuille,
                        TauxPurete        = kpis.TauxPurete
                    };
                })
                .OrderByDescending(x => x.ClientsGeres)
                .ToList();
        }

        // ═══════════════════════════════════════════════════════════
        //   HEATMAP AGENCES — Vue détaillée
        // ═══════════════════════════════════════════════════════════
        public List<TerritoireRow> ComputeHeatmapAgences(
            List<SegmentationDistributiveData> data,
            ReglesHypothesesModel? regles)
        {
            if (data == null || !data.Any())
                return new List<TerritoireRow>();

            return data
                .Where(x => !string.IsNullOrWhiteSpace(x.LibAgence))
                .GroupBy(x => new { x.LibAgence, x.LibSecteur, x.LibRegion })
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
                        ConseillersCibles = kpis.ConseillersCibles,
                        EtpNecessaires    = kpis.EtpNecessaires,
                        TauxCouverture    = kpis.TauxCouverture,
                        Concordance       = kpis.Concordance,
                        TauxRotation      = kpis.TauxRotationPortefeuille,
                        TauxPurete        = kpis.TauxPurete
                    };
                })
                .OrderByDescending(x => x.ClientsGeres)
                .ToList();
        }

        // ═══════════════════════════════════════════════════════════
        //   RÉPARTITION PAR SEGMENT
        //   Ordre métier respecté (BP → HDG → CI → GP)
        // ═══════════════════════════════════════════════════════════
        public List<SegmentRepartition> ComputeRepartitionParSegment(
            List<SegmentationDistributiveData> data)
        {
            if (data == null || !data.Any())
                return new List<SegmentRepartition>();

            var segmentsDef = new (string Nom, Func<SegmentationDistributiveData, int> Get)[]
            {
                ("HDG Premium Potentiel", x => x.HDGPremiumPotentiel),
                ("HDG Premium Standard",  x => x.HDGPremiumStandard),
                ("HDG Potentiel",         x => x.HDGPotentiel),
                ("HDG Senior Epargnant",  x => x.HDGSeniorEpargnant),
                ("HDG Standard",          x => x.HDGStandard),
                ("CI Potentiel",          x => x.CIPotentiel),
                ("CI Standard",           x => x.CIStandard),
                ("GP Potentiel",          x => x.GPPotentiel),
                ("GP Standard",           x => x.GPStandard)
            };

            var totalClients = segmentsDef.Sum(s => data.Sum(x => s.Get(x)));
            if (totalClients == 0) return new List<SegmentRepartition>();

            // Ordre naturel du segmentsDef conservé (hiérarchie métier)
            return segmentsDef
                .Select(s =>
                {
                    var nbClients = data.Sum(x => s.Get(x));

                    var chargeEtp = _etpService.GetChargeTotaleEtpForSegment(data, s.Nom) ?? 0;

                    var conseillersConcernes = data
                        .Where(x => s.Get(x) > 0)
                        .Where(x => !string.IsNullOrWhiteSpace(x.MatriculeConseiller))
                        .Select(x => x.MatriculeConseiller)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Count();

                    return new SegmentRepartition
                    {
                        Segment = s.Nom,
                        NbClients = nbClients,
                        Pourcentage = Math.Round((double)nbClients / totalClients * 100, 1),
                        ChargeEtp = Math.Round(chargeEtp, 1),
                        ConseillersConcernes = conseillersConcernes
                    };
                })
                .Where(s => s.NbClients > 0)
                .ToList();
        }

        // ═══════════════════════════════════════════════════════════
        //   DISTRIBUTION GÉOGRAPHIQUE PAR SEGMENT
        //   Segment × Dimension (Region / Secteur / Agence)
        // ═══════════════════════════════════════════════════════════
        public (Dictionary<string, Dictionary<string, int>> Distribution, List<string> Colonnes)
            ComputeDistributionGeographique(
                List<SegmentationDistributiveData> data,
                string dimension = "Region")
        {
            if (data == null || !data.Any())
                return (new(), new());

            Func<SegmentationDistributiveData, string?> selector = dimension switch
            {
                "Secteur" => x => x.LibSecteur,
                "Agence"  => x => x.LibAgence,
                _         => x => x.LibRegion
            };

            var colonnes = data
                .Select(selector)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v)
                .ToList()!;

            var segmentsDef = new (string Nom, Func<SegmentationDistributiveData, int> Get)[]
            {
                ("HDG Premium Potentiel", x => x.HDGPremiumPotentiel),
                ("HDG Premium Standard",  x => x.HDGPremiumStandard),
                ("HDG Potentiel",         x => x.HDGPotentiel),
                ("HDG Senior Epargnant",  x => x.HDGSeniorEpargnant),
                ("HDG Standard",          x => x.HDGStandard),
                ("CI Potentiel",          x => x.CIPotentiel),
                ("CI Standard",           x => x.CIStandard),
                ("GP Potentiel",          x => x.GPPotentiel),
                ("GP Standard",           x => x.GPStandard)
            };

            var distribution = new Dictionary<string, Dictionary<string, int>>();

            foreach (var seg in segmentsDef)
            {
                var byCol = new Dictionary<string, int>();

                foreach (var col in colonnes!)
                {
                    var nb = data
                        .Where(x => string.Equals(selector(x), col, StringComparison.OrdinalIgnoreCase))
                        .Sum(x => seg.Get(x));

                    byCol[col] = nb;
                }

                distribution[seg.Nom] = byCol;
            }

            return (distribution, colonnes);
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
                        ConseillersCibles = kpis.ConseillersCibles,
                        EtpNecessaires    = kpis.EtpNecessaires,
                        TauxCouverture    = kpis.TauxCouverture,
                        Concordance       = kpis.Concordance,
                        TauxRotation      = kpis.TauxRotationPortefeuille,
                        TauxPurete        = kpis.TauxPurete
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

            var segmentsBP = new[] { "HDG Premium Potentiel", "HDG Premium Standard" };
            var segmentsRetail = new[]
            {
                "HDG Potentiel", "HDG Senior Epargnant", "HDG Standard",
                "CI Potentiel", "CI Standard",
                "GP Potentiel", "GP Standard"
            };

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

            var clientsBP = data.Sum(x => x.HDGPremiumPotentiel + x.HDGPremiumStandard);
            var clientsRetail = data.Sum(x =>
                x.HDGPotentiel + x.HDGSeniorEpargnant + x.HDGStandard
                + x.CIPotentiel + x.CIStandard
                + x.GPPotentiel + x.GPStandard);

            var etpBesoinBP = segmentsBP
                .Select(seg => _etpService.GetChargeTotaleEtpForSegment(data, seg) ?? 0)
                .Sum();

            var etpBesoinRetail = segmentsRetail
                .Select(seg => _etpService.GetChargeTotaleEtpForSegment(data, seg) ?? 0)
                .Sum();

            var conseillersCiblesBP     = (int)Math.Round(etpBesoinBP);
            var conseillersCiblesRetail = (int)Math.Round(etpBesoinRetail);

            var taillePfExistBP = conseillersExistantsBP > 0
                ? (double)clientsBP / conseillersExistantsBP : 0;
            var taillePfCibleBP = conseillersCiblesBP > 0
                ? (double)clientsBP / conseillersCiblesBP : 0;

            var taillePfExistRetail = conseillersExistantsRetail > 0
                ? (double)clientsRetail / conseillersExistantsRetail : 0;
            var taillePfCibleRetail = conseillersCiblesRetail > 0
                ? (double)clientsRetail / conseillersCiblesRetail : 0;

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

        /// <summary>
        /// Taux de pureté des portefeuilles = % de clients gérés
        /// par le profil de conseiller "prioritaire" désigné en R&H
        /// pour leur segment distributif.
        /// </summary>
        private double ComputeTauxPurete(
            List<SegmentationDistributiveData> data,
            ReglesHypothesesModel? regles)
        {
            if (regles == null || data == null || !data.Any())
                return 0;

            var segmentsDef = new (string Nom, Func<SegmentationDistributiveData, int> Get)[]
            {
                ("HDG Premium Potentiel", x => x.HDGPremiumPotentiel),
                ("HDG Premium Standard",  x => x.HDGPremiumStandard),
                ("HDG Potentiel",         x => x.HDGPotentiel),
                ("HDG Senior Epargnant",  x => x.HDGSeniorEpargnant),
                ("HDG Standard",          x => x.HDGStandard),
                ("CI Potentiel",          x => x.CIPotentiel),
                ("CI Standard",           x => x.CIStandard),
                ("GP Potentiel",          x => x.GPPotentiel),
                ("GP Standard",           x => x.GPStandard)
            };

            var totalClients = 0;
            var clientsPurs  = 0;

            foreach (var seg in segmentsDef)
            {
                var regle = regles.ReglesAffectationSegments
                    .FirstOrDefault(r => string.Equals(
                        r.Segment?.Trim(), seg.Nom, StringComparison.OrdinalIgnoreCase));

                var profilPrio = regle?.ConseillerPrioritaire?.Trim();
                if (string.IsNullOrWhiteSpace(profilPrio)) continue;

                var nbSegment = data.Sum(x => seg.Get(x));
                var nbSurPrio = data
                    .Where(x => string.Equals(
                        x.TypeConseiller?.Trim(), profilPrio, StringComparison.OrdinalIgnoreCase))
                    .Sum(x => seg.Get(x));

                totalClients += nbSegment;
                clientsPurs  += nbSurPrio;
            }

            return totalClients > 0 ? (double)clientsPurs / totalClients * 100.0 : 0;
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
        public double TauxPurete { get; set; }
    }

    public class TerritoireRow
    {
        public string Nom { get; set; } = "";
        public string? Secteur { get; set; }
        public string? Region { get; set; }

        public int ClientsGeres { get; set; }

        // Conseillers
        public int ConseillersActifs { get; set; }        // = existants
        public int ConseillersCibles { get; set; }

        public double EtpNecessaires { get; set; }

        // Indicateurs de performance
        public double TauxCouverture { get; set; }
        public double Concordance { get; set; }
        public double TauxRotation { get; set; }
        public double TauxPurete { get; set; }

        public int DeltaConseillers => ConseillersActifs - ConseillersCibles;
    }

    public class SegmentRepartition
    {
        public string Segment { get; set; } = "";
        public int NbClients { get; set; }
        public double Pourcentage { get; set; }

        public double ChargeEtp { get; set; }
        public int ConseillersConcernes { get; set; }
    }

    public class LigneMetierRepartition
    {
        public string LigneMetier { get; set; } = "";
        public int NbClients { get; set; }

        public int ConseillersExistants { get; set; }
        public int ConseillersCibles { get; set; }

        public double TaillePortefeuilleExistant { get; set; }
        public double TaillePortefeuilleCible { get; set; }

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
