public class SegmentationDistributiveResponse
{
    public int SegmentationDistributiveID { get; set; }

    public string LibRegion { get; set; } = "";
    public string CodeRegion { get; set; } = "";
    public string LibSecteur { get; set; } = "";
    public string LibAgence { get; set; } = "";
    public string CodeAgence { get; set; } = "";
    public string MatriculeConseiller { get; set; } = "";
    public string TypeConseiller { get; set; } = "";

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
