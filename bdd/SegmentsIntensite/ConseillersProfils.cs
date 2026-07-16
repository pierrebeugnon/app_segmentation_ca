namespace Segmentation.Core.Entities
{
    public class ConseillersProfils : EntityBase
    {
        public int ConseillerProfilID { get; set; }

        public required string LigneMetier { get; set; }
        public required string Profil { get; set; }

        public double PartTempsCommercialPct { get; set; }
    }
}
