using CsvHelper.Configuration.Attributes;

namespace Segmentation.Application.Commands.SegmentationDistributive
{
    public class SegmentationDistributiveCsvRow
    {
        [Name("LibRegion")]              public string LibRegion { get; set; } = "";
        [Name("CodeRegion")]             public string CodeRegion { get; set; } = "";
        [Name("LibSecteur")]             public string LibSecteur { get; set; } = "";
        [Name("LibAgence")]              public string LibAgence { get; set; } = "";
        [Name("CodeAgence")]             public string CodeAgence { get; set; } = "";
        [Name("MatriculeConseiller")]    public string MatriculeConseiller { get; set; } = "";
        [Name("TypeConseiller")]         public string TypeConseiller { get; set; } = "";

        [Name("HDGPremiumPotentiel")]    public int HDGPremiumPotentiel { get; set; }
        [Name("HDGPremiumStandard")]     public int HDGPremiumStandard { get; set; }
        [Name("HDGPotentiel")]           public int HDGPotentiel { get; set; }
        [Name("HDGSeniorEpargnant")]     public int HDGSeniorEpargnant { get; set; }
        [Name("HDGStandard")]            public int HDGStandard { get; set; }

        [Name("CIPotentiel")]            public int CIPotentiel { get; set; }
        [Name("CIStandard")]             public int CIStandard { get; set; }

        [Name("GPPotentiel")]            public int GPPotentiel { get; set; }
        [Name("GPStandard")]             public int GPStandard { get; set; }

        [Name("NonSegmente")]            public int NonSegmente { get; set; }
        [Name("NonClasse")]              public int NonClasse { get; set; }
    }
}
