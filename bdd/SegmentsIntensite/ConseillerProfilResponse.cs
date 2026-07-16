namespace Segmentation.Application.Responses
{
    public class ConseillerProfilResponse
    {
        public int ConseillerProfilID { get; set; }

        public string LigneMetier { get; set; } = "";
        public string Profil { get; set; } = "";

        public double PartTempsCommercialPct { get; set; }
    }
}
