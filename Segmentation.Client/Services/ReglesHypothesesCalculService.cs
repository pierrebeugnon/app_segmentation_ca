using Segmentation.Client.Models;

namespace Segmentation.Client.Services
{
    /// <summary>
    /// Service de calcul des Règles & Hypothèses.
    /// 
    /// Objectif :
    /// - Sortir les calculs métier de ReglesHypotheses.razor
    /// - Centraliser les formules
    /// - Permettre aux autres pages, notamment Portefeuille / Dimensionnement,
    ///   de réutiliser exactement les mêmes règles.
    /// </summary>
    public class ReglesHypothesesCalculService
    {
        // ═════════════════════════════════════════════════════════════
        // VOLET 1 — Intensité relationnelle
        // Intensité = Nombre de RDV/an × Durée moyenne du RDV
        // ═════════════════════════════════════════════════════════════

        public double GetIntensite(SegmentIntensite? segment)
        {
            if (segment == null)
                return 0;

            return Math.Round(
                segment.NombreRdvParAn * segment.DureeRdvHeures,
                2
            );
        }

        public double GetIntensiteBySegment(
            ReglesHypothesesModel? model,
            string? segment)
        {
            if (model == null || string.IsNullOrWhiteSpace(segment))
                return 0;

            var segmentTrouve = model.SegmentsIntensite
                .FirstOrDefault(s =>
                    string.Equals(
                        s.Segment?.Trim(),
                        segment.Trim(),
                        StringComparison.OrdinalIgnoreCase));

            return GetIntensite(segmentTrouve);
        }

        public double GetIntensiteMoyenne(ReglesHypothesesModel? model)
        {
            if (model == null || model.SegmentsIntensite == null || !model.SegmentsIntensite.Any())
                return 0;

            var valeurs = model.SegmentsIntensite
                .Select(GetIntensite)
                .Where(v => v > 0)
                .ToList();

            return valeurs.Any()
                ? Math.Round(valeurs.Average(), 2)
                : 0;
        }

        // ═════════════════════════════════════════════════════════════
        // VOLET 2 — Temps commercial / volume horaire
        // Heures/an = Heures/semaine × Nombre de semaines/an
        // Volume commercial = Heures/an × % temps commercial
        // ═════════════════════════════════════════════════════════════

        public double GetHeuresTravailParAn(ReglesHypothesesModel? model)
        {
            if (model == null)
                return 0;

            return Math.Round(
                model.HeuresParSemaine * model.NbSemainesParAn,
                2
            );
        }

        public double ConvertPartTempsCommercialToRatio(double valeur)
        {
            if (double.IsNaN(valeur) || double.IsInfinity(valeur))
                return 0;

            if (valeur < 0)
                return 0;

            // Si la valeur est stockée en 40, 60, 70 → conversion en 0.40, 0.60, 0.70
            // Si elle est déjà stockée en 0.4 → on la conserve.
            return valeur > 1
                ? valeur / 100.0
                : valeur;
        }

        public double GetPartTempsCommercialRatio(ConseillerTempsProfil? profil)
        {
            if (profil == null)
                return 0;

            return ConvertPartTempsCommercialToRatio(profil.PartTempsCommercialPct);
        }

        public double GetPartTempsCommercialPctDisplay(ConseillerTempsProfil? profil)
        {
            return GetPartTempsCommercialRatio(profil) * 100.0;
        }

        public double GetPartTempsNonCommercialPctDisplay(ConseillerTempsProfil? profil)
        {
            return 100.0 - GetPartTempsCommercialPctDisplay(profil);
        }

        public void SetPartTempsCommercialPctFromDisplay(
            ConseillerTempsProfil? profil,
            double valeurPct)
        {
            if (profil == null)
                return;

            profil.PartTempsCommercialPct = Math.Clamp(valeurPct, 0, 100);
        }

        public double GetVolumeHoraire(
            ReglesHypothesesModel? model,
            ConseillerTempsProfil? profil)
        {
            if (model == null || profil == null)
                return 0;

            return Math.Round(
                GetHeuresTravailParAn(model) * GetPartTempsCommercialRatio(profil),
                1
            );
        }

        public double GetVolumeHoraireByProfil(
            ReglesHypothesesModel? model,
            string? profil)
        {
            if (model == null || string.IsNullOrWhiteSpace(profil))
                return 0;

            var conseiller = model.ConseillersProfils
                .FirstOrDefault(c =>
                    string.Equals(
                        c.Profil?.Trim(),
                        profil.Trim(),
                        StringComparison.OrdinalIgnoreCase));

            return GetVolumeHoraire(model, conseiller);
        }

        public double GetVolumeMoyen(ReglesHypothesesModel? model)
        {
            if (model == null || model.ConseillersProfils == null || !model.ConseillersProfils.Any())
                return 0;

            return Math.Round(
                model.ConseillersProfils.Average(c => GetVolumeHoraire(model, c)),
                1
            );
        }

        // ═════════════════════════════════════════════════════════════
        // VOLET 4 — Taille théorique des portefeuilles
        //
        // Important :
        // Le volet 4 ne dépend PAS du volet 3.
        // Il dépend uniquement :
        // - du volet 1 : intensité relationnelle des segments
        // - du volet 2 : volume horaire commercial des profils
        //
        // Formule :
        // Taille théorique = Volume horaire annuel / Intensité relationnelle
        // ═════════════════════════════════════════════════════════════

        public double GetTailleTheoriqueCalculee(
            ReglesHypothesesModel? model,
            string? profil,
            string? segment)
        {
            if (model == null)
                return 0;

            if (string.IsNullOrWhiteSpace(profil) || IsSegmentNonRenseigne(segment))
                return 0;

            var intensite = GetIntensiteBySegment(model, segment);
            var volume = GetVolumeHoraireByProfil(model, profil);

            return intensite > 0
                ? Math.Round(volume / intensite, 2)
                : 0;
        }

        /// <summary>
        /// Construit dynamiquement les lignes du volet 4 à partir des volets 1 et 2.
        ///
        /// Comme le volet 4 ne dépend pas des règles d'affectation,
        /// on croise les segments et les profils sur leur LigneMetier.
        ///
        /// Exemple :
        /// - Segment retail
        /// - Profils retail
        /// => lignes de taille théorique pour ces couples segment/profil.
        /// </summary>
        public List<TailleTheoriqueRow> BuildTaillesCalculees(ReglesHypothesesModel? model)
        {
            if (model == null)
                return new List<TailleTheoriqueRow>();

            if (model.SegmentsIntensite == null || model.ConseillersProfils == null)
                return new List<TailleTheoriqueRow>();

            var lignes = new List<TailleTheoriqueRow>();

            foreach (var segment in model.SegmentsIntensite)
            {
                if (string.IsNullOrWhiteSpace(segment.Segment))
                    continue;

                var ligneMetierSegment = Normalize(segment.LigneMetier);

                var profilsCompatibles = model.ConseillersProfils
                    .Where(p =>
                        !string.IsNullOrWhiteSpace(p.Profil)
                        && Normalize(p.LigneMetier) == ligneMetierSegment)
                    .ToList();

                foreach (var profil in profilsCompatibles)
                {
                    lignes.Add(new TailleTheoriqueRow
                    {
                        LigneMetier = profil.LigneMetier,
                        Profil = profil.Profil,
                        SegmentCouvert = segment.Segment
                    });
                }
            }

            return lignes
                .OrderBy(x => x.LigneMetier)
                .ThenBy(x => x.Profil)
                .ThenBy(x => x.SegmentCouvert)
                .ToList();
        }

        public double GetCapaciteMoyenne(ReglesHypothesesModel? model)
        {
            if (model == null)
                return 0;

            var valeurs = BuildTaillesCalculees(model)
                .Where(t => !IsSegmentNonRenseigne(t.SegmentCouvert))
                .Select(t => GetTailleTheoriqueCalculee(model, t.Profil, t.SegmentCouvert))
                .Where(v => v > 0)
                .ToList();

            return valeurs.Any()
                ? Math.Round(valeurs.Average(), 2)
                : 0;
        }

        // ═════════════════════════════════════════════════════════════
        // Helpers génériques
        // ═════════════════════════════════════════════════════════════

        public bool IsSegmentNonRenseigne(string? segment)
        {
            return string.IsNullOrWhiteSpace(segment)
                   || segment.Trim() == "?";
        }

        private static string Normalize(string? value)
        {
            return value?.Trim().ToUpperInvariant() ?? string.Empty;
        }
    }
}
