namespace Segmentation.Shared.Models
{
    public class SegmentIntensiteData
    {
        public int SegmentIntensiteID { get; set; }

        public string LigneMetier { get; set; } = "";
        public string Segment { get; set; } = "";

        public double NombreRdvParAn { get; set; }
        public double DureeRdvHeures { get; set; }

        public double IntensiteRelationnelle { get; set; }
    }
}
