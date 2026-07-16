namespace Segmentation.Shared.Models
{
    public class RegleAffectationSegmentData
    {
        public int RegleAffectationID { get; set; }

        public string Segment { get; set; } = "";
        public string ConseillerPrioritaire { get; set; } = "";
        public string ConseillerSecondaire { get; set; } = "";
        public string ConseillerTertiaire { get; set; } = "";
    }
}
