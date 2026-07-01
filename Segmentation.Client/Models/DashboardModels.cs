namespace Segmentation.Client.Models
{
    public class KpiGlobal
    {
        public int TotalClients { get; set; }
        public int TotalConseillers { get; set; }
        public int TaillePortefeuilleMoyenne { get; set; }
    }

    public class ClientParSegment
    {
        public string Segment { get; set; } = "";
        public int NombreClients { get; set; }
        public double Pourcentage { get; set; }
    }

    public class ProfilConseiller
    {
        public string Profil { get; set; } = "";
        public int NombreConseillers { get; set; }
        public double Pourcentage { get; set; }
    }

    public class TaillePortefeuille
    {
        public string Profil { get; set; } = "";
        public int ClientsParConseiller { get; set; }
        public double TauxRemplissage { get; set; }
    }

    public class IndicateurApplicabilite
    {
        public string Libelle { get; set; } = "";
        public double Valeur { get; set; }
        public bool EstPositif { get; set; }
        public bool EstPourcentage { get; set; } = true;
    }

    public class AgenceProblematique
    {
        public string NomAgence { get; set; } = "";
        public int ScoreGlobal { get; set; }
        public string NiveauScore { get; set; } = "warning";
        public double Remplissage { get; set; }
        public double ConcordanceSegment { get; set; }
        public double ConcordanceProfil { get; set; }
        public double EvolutionEffectifs { get; set; }
        public double RotationClients { get; set; }
    }

    public class DimensionnementClientRow
    {
        public string Segment { get; set; } = "";
        public int? Mutualise { get; set; }
        public int? Dedie { get; set; }
        public int? DedieHautDeGamme { get; set; }
        public int Total { get; set; }
        public bool IsTotal { get; set; } = false;
    }

    public class DimensionnementEtpRow
    {
        public string Libelle { get; set; } = "";
        public double? Mutualise { get; set; }
        public double? Dedie { get; set; }
        public double? DedieHautDeGamme { get; set; }
        public double? Total { get; set; }
        public double? Concordance { get; set; }
        public bool IsHeader { get; set; } = false;
        public bool IsBold { get; set; } = false;
        public bool IsEcart { get; set; } = false;
    }

    public class CritereApproche
    {
        public string Segment { get; set; } = "";
        public int FrequenceRdvParAn { get; set; }
        public int DureeRdvMin { get; set; }
    }

    public class PortefeuilleTheoriqueCard
    {
        public string Profil { get; set; } = "";
        public int ClientsParConseiller { get; set; }
        public string CssClass { get; set; } = "";
    }

    public class RegleAffectation
    {
        public string ProfilPortefeuille { get; set; } = "";
        public string CorrespondanceIdeale { get; set; } = "";
        public string AutreCorrespondance { get; set; } = "";
        public int SurchargeMax { get; set; }
        public int SousChargeMax { get; set; }
    }

    // ── Section 1 : Segments et intensité relationnelle ──────────────────────
    public class SegmentIntensite
    {
        public string LigneMetier { get; set; } = "";
        public string Segment { get; set; } = "";
        public double NombreRdvParAn { get; set; }
        public double DureeRdvHeures { get; set; }
        public double IntensiteRelationnelle => Math.Round(NombreRdvParAn * DureeRdvHeures, 2);
        /// <summary>Nombre de clients dans ce segment (par agence) — utilisé pour le calcul ETP cible</summary>
        public int NombreClients { get; set; }
        // Dimensions organisationnelles (filtres Portefeuille)
        public string Region  { get; set; } = "";
        public string Secteur { get; set; } = "";
        public string Agence  { get; set; } = "";
    }

    // ── Section 2 : Conseillers et temps commercial ───────────────────────────
    public class ConseillerTempsProfil
    {
        public string LigneMetier { get; set; } = "";
        public string Profil { get; set; } = "";
        public double PartTempsCommercialPct { get; set; }
        public double PartTempsNonCommercialPct => 100.0 - PartTempsCommercialPct;
    }

    // ── Section 3 : Règles d'affectation par segment ─────────────────────────
    public class RegleAffectationSegment
    {
        public string Segment { get; set; } = "";
        public string ConseillerPrioritaire { get; set; } = "";
        public string ConseillerSecondaire { get; set; } = "";
        public string ConseillerTertiaire { get; set; } = "";
    }

    // ── Section 4 : Taille théorique (config fixe, calculs dynamiques) ────────
    public class TailleTheoriqueRow
    {
        public string LigneMetier { get; set; } = "";
        public string Profil { get; set; } = "";
        public string SegmentCouvert { get; set; } = "";
    }

    public class ReglesHypothesesModel
    {
        // ── Section 1
        public List<SegmentIntensite> SegmentsIntensite { get; set; } = new();

        // ── Section 2
        public double HeuresParSemaine { get; set; } = 37.3;
        public double NbSemainesParAn { get; set; } = 42.5;
        public double HeuresTravailParAn => Math.Round(HeuresParSemaine * NbSemainesParAn, 2);
        public List<ConseillerTempsProfil> ConseillersProfils { get; set; } = new();

        // ── Section 3
        public List<RegleAffectationSegment> ReglesAffectationSegments { get; set; } = new();

        // ── Section 4 (config profil→segment, valeurs calculées à l'affichage)
        public List<TailleTheoriqueRow> TaillesTheoretiques { get; set; } = new();

        // ── Backward compat (utilisés par SegmentationStateService)
        public List<CritereApproche> CriteresApproche { get; set; } = new();
        public int PartTempsCommercial { get; set; } = 40;
        public List<PortefeuilleTheoriqueCard> PortefeuillesTheoriques { get; set; } = new();
        public List<RegleAffectation> ReglesAffectation { get; set; } = new();
    }

    // ── Conseiller individuel (slot) dans le scénario de dimensionnement ─────
    public class ConseillerSlot
    {
        public string Id       { get; set; } = Guid.NewGuid().ToString("N")[..8];
        /// <summary>Type de profil, ex. "BANQUIER PRIVÉ"</summary>
        public string Profil   { get; set; } = "";
        /// <summary>Label court affiché dans le tableau, ex. "BP-2"</summary>
        public string Label    { get; set; } = "";
        /// <summary>Présent dans l'effectif actuel</summary>
        public bool   IsActuel { get; set; } = true;
        /// <summary>Conservé dans le scénario cible (false = grisé dans le tableau)</summary>
        public bool   IsCible  { get; set; } = true;
    }

    // ── Vue Dimensionnement fusionnée (ETP + Volume) ──────────────────────────
    public class DimMatrixRow
    {
        public string Segment { get; set; } = "";
        /// <summary>ETP EXISTANT — valeur réelle actuelle (lecture seule dans l'UI)</summary>
        public Dictionary<string, double?> EtpParProfil { get; set; } = new();
        /// <summary>ETP CIBLE — valeur saisie par l'utilisateur (modifiable dans l'UI)</summary>
        public Dictionary<string, double?> EtpCibleSaisieParProfil { get; set; } = new();
        public bool IsTotal { get; set; } = false;
    }

    public class FichierSource
    {
        public string Titre { get; set; } = "";
        public string Description { get; set; } = "";
        public string NomFichier { get; set; } = "";
        public bool EstCharge { get; set; } = false;
        public DateTime? DateChargement { get; set; }
    }

    public class VisionGlobaleModel
    {
        public KpiGlobal Kpis { get; set; } = new();
        public List<ClientParSegment> ClientsParSegment { get; set; } = new();
        public List<ProfilConseiller> ProfilsConseillers { get; set; } = new();
        public List<TaillePortefeuille> TaillesPortefeuilles { get; set; } = new();
        public List<IndicateurApplicabilite> Indicateurs { get; set; } = new();
        public List<AgenceProblematique> AgencesProblematiques { get; set; } = new();
    }
}
