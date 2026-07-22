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

            // 1. Conseillers actifs (matricules distincts)
            var conseillersActifs = data
                .Where(x => !string.IsNullOrWhiteSpace(x.MatriculeConseiller))
                .Select(x => x.MatriculeConseiller)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            // 2. Clients gérés (somme de tous les segments)
            var clientsGeres =
                data.Sum(x => x.HDGPremiumPotentiel + x.HDGPremiumStandard
                            + x.HDGPotentiel + x.HDGSeniorEpargnant + x.HDGStandard
                            + x.CIPotentiel + x.CIStandard
                            + x.GPPotentiel + x.GPStandard
                            + x.NonSegmente + x.NonClasse);

            // 3. ETP nécessaires (somme des charges théoriques par segment)
            var etpNecessaires = ComputeTotalEtpBesoin(data);

            // 4. Taux de couverture
            var tauxCouverture = etpNecessaires > 0
                ? conseillersActifs / etpNecessaires * 100.0
                : 0;

            // 5. Concordance moyenne (approximée à ce stade — sera affinée)
            var concordance = ComputeConcordanceMoyenne(data, regles);

            return new DashboardKpis
            {
                ConseillersActifs = conseillersActifs,
                ClientsGeres = clientsGeres,
                EtpNecessaires = Math.Round(etpNecessaires, 1),
                TauxCouverture = Math.Round(tauxCouverture, 0),
                Concordance = Math.Round(concordance, 0)
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

            // Décide du niveau de granularité
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
                        Nom = g.Key,
                        ClientsGeres = kpis.ClientsGeres,
                        ConseillersActifs = kpis.ConseillersActifs,
                        EtpNecessaires = kpis.EtpNecessaires,
                        TauxCouverture = kpis.TauxCouverture,
                        Concordance = kpis.Concordance
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
                        Nom = g.Key.LibAgence,
                        Secteur = g.Key.LibSecteur,
                        Region = g.Key.LibRegion,
                        ClientsGeres = kpis.ClientsGeres,
                        ConseillersActifs = kpis.ConseillersActifs,
                        EtpNecessaires = kpis.EtpNecessaires,
                        TauxCouverture = kpis.TauxCouverture,
                        Concordance = kpis.Concordance
                    };
                })
                .OrderBy(x => x.Concordance)   // les plus faibles = alerte
                .Take(top)
                .ToList();
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
            // Placeholder V1 — calcul simplifié
            // À affiner en V2 en croisant avec les règles d'affectation
            if (regles == null || data == null || !data.Any())
                return 0;

            // Approche V1 : % de conseillers dont le type match une règle prioritaire
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
        public int ConseillersActifs { get; set; }
        public int ClientsGeres { get; set; }
        public double EtpNecessaires { get; set; }
        public double TauxCouverture { get; set; }
        public double Concordance { get; set; }
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
}
