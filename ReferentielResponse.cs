namespace Segmentation.Application.Responses
{
    public class ReferentielResponse
    {
        public List<string> Segments { get; set; } = new();
        public List<ReferentielProfilResponse> Profils { get; set; } = new();
        public List<string> Regions { get; set; } = new();
        public List<string> Secteurs { get; set; } = new();
        public List<string> Agences { get; set; } = new();
    }

    public class ReferentielProfilResponse
    {
        public string Profil { get; set; } = "";
        public string LigneMetier { get; set; } = "";
    }
}
