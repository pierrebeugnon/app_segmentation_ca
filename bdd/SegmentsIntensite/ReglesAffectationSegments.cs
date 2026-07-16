namespace Segmentation.Core.Entities
{
    public class ReglesAffectationSegments : EntityBase
    {
        public int RegleAffectationID { get; set; }

        public required string Segment { get; set; }

        public string? ConseillerPrioritaire { get; set; }
        public string? ConseillerSecondaire { get; set; }
        public string? ConseillerTertiaire { get; set; }
    }
}
