namespace Segmentation.Shared.Models.DimensionnementPortefeuille;

public class DimensionnementPortefeuilleEtpChargeDto
{
    public string Segment { get; set; } = string.Empty;

    public double? ChargeATransfererBP { get; set; }

    public double? ChargeRecueBP { get; set; }

    public double? ChargeATransfererMutualise { get; set; }
}
