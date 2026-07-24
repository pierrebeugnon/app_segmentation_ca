// Fichier shim local : dans le repo client complet, ces modèles proviennent
// du projet Segmentation.Shared. Dans notre repo réduit, on les redéclare ici
// sous le même namespace pour permettre la compilation.
// SUPPRIMER CE FICHIER lors du merge dans le repo client.

namespace Segmentation.Shared.Models;

public class SegmentationDistributiveData
{
    public string LibRegion { get; set; } = "";
    public string LibSecteur { get; set; } = "";
    public string LibAgence { get; set; } = "";
    public string MatriculeConseiller { get; set; } = "";
    public string TypeConseiller { get; set; } = "";
    public double? Etp { get; set; }

    public int HDGPremiumPotentiel { get; set; }
    public int HDGPremiumStandard { get; set; }
    public int HDGPotentiel { get; set; }
    public int HDGSeniorEpargnant { get; set; }
    public int HDGStandard { get; set; }
    public int CIPotentiel { get; set; }
    public int CIStandard { get; set; }
    public int GPPotentiel { get; set; }
    public int GPStandard { get; set; }
    public int NonSegmente { get; set; }
    public int NonClasse { get; set; }
}

public class SegmentIntensiteData
{
    public int SegmentIntensiteID { get; set; }
    public string LigneMetier { get; set; } = "";
    public string Segment { get; set; } = "";
    public double NombreRdvParAn { get; set; }
    public double DureeRdvHeures { get; set; }
    public double IntensiteRelationnelle { get; set; }
    public int NombreClients { get; set; }
    public string Region { get; set; } = "";
    public string Secteur { get; set; } = "";
    public string Agence { get; set; } = "";
}

public class ConseillerProfilData
{
    public string LigneMetier { get; set; } = "";
    public string Profil { get; set; } = "";
    public double PartTempsCommercialPct { get; set; }
    public double PartTempsNonCommercialPct => 100.0 - PartTempsCommercialPct;
}

public class RegleAffectationSegmentData
{
    public string Segment { get; set; } = "";
    public string ConseillerPrioritaire { get; set; } = "";
    public string ConseillerSecondaire { get; set; } = "";
    public string ConseillerTertiaire { get; set; } = "";
}

public class ParametresGenerauxData
{
    public int ParametresGenerauxID { get; set; }
    public double HeuresParSemaine { get; set; }
    public double NbSemainesParAn { get; set; }
}
