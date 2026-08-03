namespace Segmentation.Shared.Models
{
    public class DimensionnementPortefeuilleEtpData
    {
        public string Segment { get; set; } = "";

        public string ProfilConseiller { get; set; } = "";

        public string? MatriculeConseiller { get; set; }

        public string SlotLabel { get; set; } = "";

        public bool IsActuel { get; set; }

        public bool IsCible { get; set; }

        public double? EtpExistant { get; set; }

        public double? EtpCible { get; set; }

        public double CapaciteEtpActuel { get; set; }

        public double CapaciteEtpCible { get; set; }
    }
}
