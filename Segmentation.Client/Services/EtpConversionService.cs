using Segmentation.Shared.Models;

namespace Segmentation.Client.Services
{
    /// <summary>
    /// Service de conversion Nombre de clients → ETP
    /// Reproduit la logique de l'Excel CANDF_TEST_Outils de dimensionnementV7
    ///
    /// Principe :
    ///   1. Calcul de la charge totale ETP par segment au niveau agence :
    ///        charge = nbClientsAgenceSegment / tailleTheoriqueSegment
    ///
    ///   2. Répartition de cette charge entre les conseillers au prorata
    ///      de leurs clients dans le segment :
    ///        etpConseiller = chargeTotale
    ///                        * clientsConseillerSegment
    ///                        / clientsAgenceSegment
    /// </summary>
    public class EtpConversionService
    {
        // ── Tailles théoriques issues de l'Excel de référence ─────────
        // Onglet "1. PARAMETRES" - Taille théorique de portefeuille
        private static readonly Dictionary<string, double> _tailleTheoriqueParSegment =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["HDG Premium Potentiel"] = 190.23,
                ["HDG Premium Standard"]  = 237.7875,

                ["HDG Potentiel"]         = 181.17142857142855,
                ["HDG Senior Epargnant"]  = 281.8222222222222,
                ["HDG Standard"]          = 281.8222222222222,

                ["CI Potentiel"]          = 253.64,
                ["CI Standard"]           = 507.28,

                ["GP Potentiel"]          = 507.28,
                ["GP Standard"]           = 2536.4
            };

        // ── Public API ────────────────────────────────────────────────

        /// <summary>
        /// Charge totale ETP d'un segment sur le périmètre donné (agence, secteur, région)
        /// </summary>
        public double? GetChargeTotaleEtpForSegment(
            IReadOnlyList<SegmentationDistributiveData> perimetre,
            string segment)
        {
            if (perimetre is null || perimetre.Count == 0)
                return null;

            var clientsTotal = perimetre.Sum(x => GetClientCountForSegment(x, segment));

            if (clientsTotal <= 0)
                return null;

            var taille = GetTailleTheoriqueForSegment(segment);

            if (!taille.HasValue || taille.Value <= 0)
                return null;

            return Math.Round(clientsTotal / taille.Value, 4);
        }

        /// <summary>
        /// ETP porté par un conseiller sur un segment donné,
        /// calculé au prorata de ses clients / total agence.
        /// </summary>
        public double? GetEtpForConseiller(
            IReadOnlyList<SegmentationDistributiveData> perimetre,
            string segment,
            string matriculeConseiller)
        {
            if (perimetre is null || perimetre.Count == 0)
                return null;

            if (string.IsNullOrWhiteSpace(matriculeConseiller))
                return null;

            var clientsAgence = perimetre.Sum(x => GetClientCountForSegment(x, segment));
            if (clientsAgence <= 0)
                return null;

            var clientsConseiller = perimetre
                .Where(x => string.Equals(
                    x.MatriculeConseiller,
                    matriculeConseiller,
                    StringComparison.OrdinalIgnoreCase))
                .Sum(x => GetClientCountForSegment(x, segment));

            if (clientsConseiller <= 0)
                return null;

            var chargeTotale = GetChargeTotaleEtpForSegment(perimetre, segment);
            if (!chargeTotale.HasValue || chargeTotale.Value <= 0)
                return null;

            return Math.Round(
                chargeTotale.Value * clientsConseiller / clientsAgence,
                4
            );
        }

        /// <summary>
        /// Nombre de clients d'une ligne pour un segment donné.
        /// </summary>
        public int GetClientCountForSegment(
            SegmentationDistributiveData row,
            string segment)
        {
            if (row is null)
                return 0;

            return NormalizeSegment(segment) switch
            {
                "HDG Premium Potentiel" => row.HDGPremiumPotentiel,
                "HDG Premium Standard"  => row.HDGPremiumStandard,
                "HDG Potentiel"         => row.HDGPotentiel,
                "HDG Senior Epargnant"  => row.HDGSeniorEpargnant,
                "HDG Standard"          => row.HDGStandard,

                "CI Potentiel"          => row.CIPotentiel,
                "CI Standard"           => row.CIStandard,

                "GP Potentiel"          => row.GPPotentiel,
                "GP Standard"           => row.GPStandard,

                "Non segmenté"          => row.NonSegmente,
                "Non classé"            => row.NonClasse,

                _ => 0
            };
        }

        /// <summary>
        /// Nombre de clients correspondant à un ETP donné pour un segment.
        /// </summary>
        public double? GetClientCountFromEtp(double? etp, string segment)
        {
            if (!etp.HasValue || etp.Value <= 0)
                return null;

            var taille = GetTailleTheoriqueForSegment(segment);
            if (!taille.HasValue || taille.Value <= 0)
                return null;

            return Math.Round(etp.Value * taille.Value, 2);
        }

        /// <summary>
        /// Taille théorique de portefeuille pour un segment.
        /// </summary>
        public double? GetTailleTheoriqueForSegment(string segment)
        {
            var key = NormalizeSegment(segment);

            return _tailleTheoriqueParSegment.TryGetValue(key, out var taille)
                ? taille
                : null;
        }

        // ── Helpers ────────────────────────────────────────────────────

        private static string NormalizeSegment(string segment)
        {
            return (segment ?? string.Empty).Trim();
        }
    }
}
