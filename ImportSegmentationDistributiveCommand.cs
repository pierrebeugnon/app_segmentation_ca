using MediatR;

namespace Segmentation.Application.Commands.SegmentationDistributive
{
    public class ImportSegmentationDistributiveCommand : IRequest<int>
    {
        public required Stream CsvStream { get; set; }
        public bool ReplaceExisting { get; set; } = false;
    }
}
