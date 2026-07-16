using MediatR;

namespace Segmentation.Application.Commands.RegleAffectationSegment
{
    public class SaveReglesAffectationSegmentsCommand : IRequest<int>
    {
        public List<SaveRegleAffectationSegmentItem> Items { get; set; } = new();
    }

    public class SaveRegleAffectationSegmentItem
    {
        public string Segment { get; set; } = "";
        public string ConseillerPrioritaire { get; set; } = "";
        public string ConseillerSecondaire { get; set; } = "";
        public string ConseillerTertiaire { get; set; } = "";
    }
}
