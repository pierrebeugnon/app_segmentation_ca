using Segmentation.Client.Models;

namespace Segmentation.Client.Services;

/// <summary>
/// Modèle de répartition automatique de la charge cible d'un conseiller retiré de la
/// répartition cible, utilisé par la modale de retrait d'effectif de la page
/// "Dimensionnement des Portefeuilles". Séparé de la page pour ne pas l'alourdir davantage.
///
/// Règles :
///  - La charge est replacée en priorité sur les conseillers désignés dans les
///    Règles &amp; Hypothèses (Prioritaire, puis Secondaire, puis Tertiaire) pour le
///    segment concerné.
///  - Si aucun conseiller prioritaire n'est disponible dans l'effectif (absent ou déjà
///    au maximum), on redescend la liste de priorité du segment jusqu'au dernier
///    recours (profils non désignés, triés alphabétiquement).
///  - Aucun conseiller ne peut recevoir plus de 1 ETP de charge cible au total
///    (toutes lignes/segments confondus).
///  - Le calcul est refait pour chaque segment concerné par la charge à redistribuer.
/// </summary>
public class RepartitionAutomatiqueService
{
    public const double MaxEtpParConseiller = 1.0;

    /// <summary>
    /// Charge cible totale déjà portée par un conseiller, tous segments confondus.
    /// </summary>
    public double GetChargeCibleTotal(ConseillerSlot slot, List<DimMatrixRow> toutesLesLignes) =>
        Math.Round(toutesLesLignes
            .Where(r => !r.IsTotal)
            .Sum(r => r.EtpCibleSaisieParProfil.GetValueOrDefault(slot.Id) ?? 0), 2);

    /// <summary>Capacité restante avant d'atteindre le maximum de 1 ETP.</summary>
    public double GetCapaciteDisponible(ConseillerSlot slot, List<DimMatrixRow> toutesLesLignes) =>
        Math.Max(0, Math.Round(MaxEtpParConseiller - GetChargeCibleTotal(slot, toutesLesLignes), 2));

    /// <summary>
    /// Classe les conseillers éligibles pour un segment donné, du plus prioritaire
    /// (désigné Prioritaire dans les R&amp;H) au moins prioritaire (profil non désigné).
    /// </summary>
    public List<ConseillerSlot> GetOrdrePrioritePourSegment(
        string segment,
        List<ConseillerSlot> slotsEligibles,
        List<RegleAffectationSegment> regles)
    {
        var regle = regles.FirstOrDefault(r =>
            r.Segment?.Trim().Equals(segment?.Trim(), StringComparison.OrdinalIgnoreCase) == true);

        int NiveauPriorite(ConseillerSlot s)
        {
            if (regle == null || string.IsNullOrWhiteSpace(s.Profil)) return 4;
            if (!string.IsNullOrWhiteSpace(regle.ConseillerPrioritaire) &&
                regle.ConseillerPrioritaire.Trim().Equals(s.Profil.Trim(), StringComparison.OrdinalIgnoreCase)) return 1;
            if (!string.IsNullOrWhiteSpace(regle.ConseillerSecondaire) &&
                regle.ConseillerSecondaire.Trim().Equals(s.Profil.Trim(), StringComparison.OrdinalIgnoreCase)) return 2;
            if (!string.IsNullOrWhiteSpace(regle.ConseillerTertiaire) &&
                regle.ConseillerTertiaire.Trim().Equals(s.Profil.Trim(), StringComparison.OrdinalIgnoreCase)) return 3;
            return 4; // hors liste R&H pour ce segment = dernier recours
        }

        return slotsEligibles
            .OrderBy(NiveauPriorite)
            .ThenBy(s => s.Profil, StringComparer.OrdinalIgnoreCase)
            .ThenBy(s => s.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Répartit automatiquement, segment par segment, la charge cible d'un conseiller
    /// retiré sur les conseillers éligibles (mutation directe des lignes fournies).
    /// </summary>
    /// <param name="memeTypeUniquement">
    /// Si vrai, seuls les conseillers du même profil que celui retiré sont éligibles
    /// (option "type identique" de la modale). Sinon, tous les profils sont éligibles
    /// suivant l'ordre de priorité du segment.
    /// </param>
    /// <returns>Le solde (ETP) n'ayant pas pu être replacé, par ligne/segment concerné.</returns>
    public Dictionary<DimMatrixRow, double> RepartirAutomatiquement(
        ConseillerSlot slotARetirer,
        List<DimMatrixRow> segmentsAvecCharge,
        List<DimMatrixRow> toutesLesLignes,
        List<ConseillerSlot> tousLesSlots,
        List<RegleAffectationSegment> regles,
        bool memeTypeUniquement)
    {
        var solde = new Dictionary<DimMatrixRow, double>();

        var candidats = tousLesSlots
            .Where(s => s.Id != slotARetirer.Id && s.IsCible)
            .Where(s => !memeTypeUniquement || s.Profil.Equals(slotARetirer.Profil, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var row in segmentsAvecCharge)
        {
            var montant = Math.Round(row.EtpCibleSaisieParProfil.GetValueOrDefault(slotARetirer.Id) ?? 0, 2);
            if (montant <= 0) continue;

            var ordre = GetOrdrePrioritePourSegment(row.Segment, candidats, regles);
            var restant = montant;

            foreach (var candidat in ordre)
            {
                if (restant <= 0) break;

                var capaciteDisponible = GetCapaciteDisponible(candidat, toutesLesLignes);
                if (capaciteDisponible <= 0) continue;

                var aPrendre = Math.Round(Math.Min(restant, capaciteDisponible), 2);
                if (aPrendre <= 0) continue;

                row.EtpCibleSaisieParProfil[candidat.Id] =
                    Math.Round((row.EtpCibleSaisieParProfil.GetValueOrDefault(candidat.Id) ?? 0) + aPrendre, 2);
                restant = Math.Round(restant - aPrendre, 2);
            }

            row.EtpCibleSaisieParProfil[slotARetirer.Id] = restant > 0.001 ? restant : null;
            if (restant > 0.001)
                solde[row] = restant;
        }

        return solde;
    }

    /// <summary>
    /// Liste des conseillers disponibles (capacité &lt; 1 ETP) pour la répartition manuelle,
    /// triés du profil le plus prioritaire (pour le segment) au moins prioritaire.
    /// </summary>
    public List<ConseillerSlot> GetConseillersDisponiblesPourSegment(
        string segment,
        ConseillerSlot slotARetirer,
        List<ConseillerSlot> tousLesSlots,
        List<DimMatrixRow> toutesLesLignes,
        List<RegleAffectationSegment> regles,
        bool memeTypeUniquement = false)
    {
        var eligibles = tousLesSlots
            .Where(s => s.Id != slotARetirer.Id && s.IsCible)
            .Where(s => !memeTypeUniquement || s.Profil.Equals(slotARetirer.Profil, StringComparison.OrdinalIgnoreCase))
            .Where(s => GetCapaciteDisponible(s, toutesLesLignes) > 0.001)
            .ToList();

        return GetOrdrePrioritePourSegment(segment, eligibles, regles);
    }
}
