namespace Segmentation.Core.Entities
{
    public class SegmentsIntensite : EntityBase
    {
        public int SegmentIntensiteID { get; set; }

        public required string LigneMetier { get; set; }
        public required string Segment { get; set; }

        public double NombreRdvParAn { get; set; }
        public double DureeRdvHeures { get; set; }
    }
}
