using Segmentation.Client.Models;

namespace Segmentation.Client.Services
{
    /// <summary>
    /// Service de calcul des Règles & Hypothèses.
    ///
    /// Objectif :
    /// - Sortir les calculs métier de ReglesHypotheses.razor
    /// - Centraliser les formules
    /// - Permettre aux autres pages (Portefeuille, Dimensionnement, Vision Globale)
    ///   de réutiliser exactement les mêmes règles.
    ///
    /// Modèle métier :
    /// - Volet 1 : Intensité relationnelle des segments
    /// - Volet 2 : Temps commercial (défini par TYPE D'AGENCE : BP ou Retail)
    /// - Volet 3 : Affectation des conseillers → n'intervient PAS dans les calculs
    /// - Volet 4 : Taille théorique = Volume horaire / Intensité
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
                2);
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
        // VOLET 2 — Temps commercial et volume horaire
        //
        // Le % temps commercial est défini par TYPE D'AGENCE :
        //   - banque privée
        //   - retail
        //
        // Tous les profils d'un même type d'agence partagent le même %.
        // Donc le volume horaire est identique pour tous les profils
        // d'un même type.
        //
        // Formule :
        //   Heures/an       = HeuresParSemaine × NbSemainesParAn
        //   Volume horaire  = Heures/an × %TempsCommercial(type d'agence)
        // ═════════════════════════════════════════════════════════════

        public double GetHeuresTravailParAn(ReglesHypothesesModel? model)
        {
            if (model == null)
                return 0;

            return Math.Round(
                model.HeuresParSemaine * model.NbSemainesParAn,
                2);
        }

        public double ConvertPartTempsCommercialToRatio(double valeur)
        {
            if (double.IsNaN(valeur) || double.IsInfinity(valeur))
                return 0;

            if (valeur < 0)
                return 0;

            // Si stocké en 40, 60, 70 → conversion en 0.40, 0.60, 0.70
            // Si stocké en 0.4 → conservé
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
                1);
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
                1);
        }

        /// <summary>
        /// % temps commercial d'un type d'agence (banque privée ou retail).
        /// Tous les profils d'un même type ont le même %,
        /// donc on lit sur le premier profil trouvé.
        /// </summary>
        public double GetPartTempsCommercialPctByTypeAgence(
            ReglesHypothesesModel? model,
            string? typeAgence)
        {
            if (model == null || string.IsNullOrWhiteSpace(typeAgence))
                return 0;

            var profil = model.ConseillersProfils
                .FirstOrDefault(p =>
                    string.Equals(
                        p.LigneMetier?.Trim(),
                        typeAgence.Trim(),
                        StringComparison.OrdinalIgnoreCase));

            return profil != null
                ? GetPartTempsCommercialPctDisplay(profil)
                : 0;
        }

        /// <summary>
        /// Volume horaire annuel commercial d'un type d'agence.
        /// Formule : Heures/an × %TempsCommercial du type.
        /// </summary>
        public double GetVolumeHoraireByTypeAgence(
            ReglesHypothesesModel? model,
            string? typeAgence)
        {
            if (model == null || string.IsNullOrWhiteSpace(typeAgence))
                return 0;

            var pct = GetPartTempsCommercialPctByTypeAgence(model, typeAgence);
            var heuresAn = GetHeuresTravailParAn(model);

            return Math.Round(heuresAn * pct / 100.0, 1);
        }

        // ═════════════════════════════════════════════════════════════
        // VOLET 4 — Taille théorique des portefeuilles
        //
        // Important :
        //   - Ne dépend PAS du volet 3.
        //   - Une ligne par segment du volet 1.
        //   - Le volume horaire est celui du TYPE D'AGENCE du segment.
        //
        // Formule :
        //   Taille théorique = Volume horaire / Intensité
        // ═════════════════════════════════════════════════════════════

        /// <summary>
        /// Taille théorique d'un segment donné.
        /// Utilise directement le type d'agence du segment.
        /// </summary>
        public double GetTailleTheoriqueParSegment(
            ReglesHypothesesModel? model,
            string? segment)
        {
            if (model == null || IsSegmentNonRenseigne(segment))
                return 0;

            var segmentTrouve = model.SegmentsIntensite
                .FirstOrDefault(s =>
                    string.Equals(
                        s.Segment?.Trim(),
                        segment!.Trim(),
                        StringComparison.OrdinalIgnoreCase));

            if (segmentTrouve == null)
                return 0;

            var intensite = GetIntensite(segmentTrouve);
            if (intensite <= 0)
                return 0;

            var volume = GetVolumeHoraireByTypeAgence(
                model,
                segmentTrouve.LigneMetier);

            return Math.Round(volume / intensite, 2);
        }

        /// <summary>
        /// Version historique conservée pour compatibilité avec les autres pages.
        /// Si profil est fourni → calcul basé sur le profil.
        /// Sinon → calcul basé sur le type d'agence du segment.
        /// </summary>
        public double GetTailleTheoriqueCalculee(
            ReglesHypothesesModel? model,
            string? profil,
            string? segment)
        {
            if (model == null || IsSegmentNonRenseigne(segment))
                return 0;

            var intensite = GetIntensiteBySegment(model, segment);
            if (intensite <= 0)
                return 0;

            double volume;

            if (string.IsNullOrWhiteSpace(profil))
            {
                var segmentTrouve = model.SegmentsIntensite
                    .FirstOrDefault(s =>
                        string.Equals(
                            s.Segment?.Trim(),
                            segment!.Trim(),
                            StringComparison.OrdinalIgnoreCase));

                volume = GetVolumeHoraireByTypeAgence(
                    model,
                    segmentTrouve?.LigneMetier);
            }
            else
            {
                volume = GetVolumeHoraireByProfil(model, profil);
            }

            return Math.Round(volume / intensite, 2);
        }

        /// <summary>
        /// Construit la liste des lignes du volet 4.
        /// Une ligne par segment du volet 1, ni plus ni moins.
        /// </summary>
        public List<TailleTheoriqueRow> BuildTaillesCalculees(ReglesHypothesesModel? model)
        {
            if (model == null || model.SegmentsIntensite == null)
                return new List<TailleTheoriqueRow>();

            return model.SegmentsIntensite
                .Where(s => !string.IsNullOrWhiteSpace(s.Segment))
                .Select(s => new TailleTheoriqueRow
                {
                    LigneMetier = s.LigneMetier ?? "",
                    Profil = "",
                    SegmentCouvert = s.Segment
                })
                .ToList();
        }

        /// <summary>
        /// Capacité moyenne = moyenne des tailles théoriques par segment.
        /// </summary>
        public double GetCapaciteMoyenne(ReglesHypothesesModel? model)
        {
            if (model == null)
                return 0;

            var valeurs = BuildTaillesCalculees(model)
                .Where(t => !IsSegmentNonRenseigne(t.SegmentCouvert))
                .Select(t => GetTailleTheoriqueParSegment(model, t.SegmentCouvert))
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
    }
}
