namespace Segmentation.Application.Responses
{
    public class SegmentIntensiteResponse
    {
        public int SegmentIntensiteID { get; set; }

        public string LigneMetier { get; set; } = "";
        public string Segment { get; set; } = "";

        public double NombreRdvParAn { get; set; }
        public double DureeRdvHeures { get; set; }

        // Calculé côté serveur pour convenance
        public double IntensiteRelationnelle =>
            Math.Round(NombreRdvParAn * DureeRdvHeures, 2);
    }
}
