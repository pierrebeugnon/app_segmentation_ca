namespace Segmentation.Shared.Models.DimensionnementPortefeuille;

public class DimensionnementPortefeuilleEtpDto
{
    public string Segment { get; set; } = string.Empty;

    public string ProfilConseiller { get; set; } = string.Empty;

    public string? MatriculeConseiller { get; set; }

    public string SlotLabel { get; set; } = string.Empty;

    public bool IsActuel { get; set; }

    public bool IsCible { get; set; }

    public double? EtpExistant { get; set; }

    public double? EtpCible { get; set; }

    public double CapaciteEtpActuel { get; set; }

    public double CapaciteEtpCible { get; set; }
}
`
