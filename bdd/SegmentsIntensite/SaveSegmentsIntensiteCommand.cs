using MediatR;

namespace Segmentation.Application.Commands.SegmentIntensite
{
    public class SaveSegmentsIntensiteCommand : IRequest<int>
    {
        public List<SaveSegmentIntensiteItem> Items { get; set; } = new();
    }

    public class SaveSegmentIntensiteItem
    {
        public string LigneMetier { get; set; } = "";
        public string Segment { get; set; } = "";
        public double NombreRdvParAn { get; set; }
        public double DureeRdvHeures { get; set; }
    }
}
