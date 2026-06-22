namespace CA.SegmentationClient.Models
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

    public class ReglesHypothesesModel
    {
        public List<CritereApproche> CriteresApproche { get; set; } = new();
        public int PartTempsCommercial { get; set; }
        public List<PortefeuilleTheoriqueCard> PortefeuillesTheoriques { get; set; } = new();
        public List<RegleAffectation> ReglesAffectation { get; set; } = new();
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
