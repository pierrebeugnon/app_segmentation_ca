namespace Segmentation.Shared.Models
{
    /// <summary>
    /// Référentiel dynamique construit à partir de SegmentationDistributives.
    /// Sert de source pour les listes déroulantes et pour initialiser
    /// les Règles & Hypothèses.
    /// </summary>
    public class ReferentielData
    {
        public List<string> Segments { get; set; } = new();
        public List<ReferentielProfilData> Profils { get; set; } = new();
        public List<string> Regions { get; set; } = new();
        public List<string> Secteurs { get; set; } = new();
        public List<string> Agences { get; set; } = new();
    }

    public class ReferentielProfilData
    {
        public string Profil { get; set; } = "";
        public string LigneMetier { get; set; } = "";  // "banque privée" ou "retail"
    }
}
