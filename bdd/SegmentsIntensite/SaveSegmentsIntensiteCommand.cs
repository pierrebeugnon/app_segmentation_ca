using Segmentation.Application.Contracts;
using Segmentation.Shared.Models;

namespace Segmentation.Application.Commands.SegmentIntensite
{
    public class SaveSegmentsIntensiteCommand : ICommand<int>
    {
        public required List<SegmentIntensiteData> Items { get; set; }
    }
}
