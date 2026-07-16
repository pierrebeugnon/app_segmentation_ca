using MediatR;
using Segmentation.Shared.Models;

namespace Segmentation.Application.Commands.SegmentIntensite
{
    public class SaveSegmentsIntensiteCommand : IRequest<int>
    {
        public required List<SegmentIntensiteData> Items { get; set; }
    }
}
