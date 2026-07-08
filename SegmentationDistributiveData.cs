namespace Segmentation.Shared.Models
{
    public class SegmentationDistributiveData
    {
        public int SegmentationDistributiveID { get; set; }

        public string LibRegion { get; set; } = string.Empty;
        public string CodeRegion { get; set; } = string.Empty;
        public string LibSecteur { get; set; } = string.Empty;
        public string LibAgence { get; set; } = string.Empty;
        public string CodeAgence { get; set; } = string.Empty;
        public string MatriculeConseiller { get; set; } = string.Empty;
        public string TypeConseiller { get; set; } = string.Empty;

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
}
