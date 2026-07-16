namespace Segmentation.Core.Entities
{
    public class ParametresGeneraux : EntityBase
    {
        public int ParametresGenerauxID { get; set; }

        public double HeuresParSemaine { get; set; }
        public double NbSemainesParAn { get; set; }
    }
}
