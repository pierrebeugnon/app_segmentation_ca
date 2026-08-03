namespace Segmentation.Shared.Models.DimensionnementPortefeuille;

public class SaveDimensionnementPortefeuilleEtpRequest
{
    public string? LibRegion { get; set; }

    public string? LibSecteur { get; set; }

    public string LibAgence { get; set; } = string.Empty;

    public List<DimensionnementPortefeuilleEtpDto> Lignes { get; set; }
        = new();

    public List<DimensionnementPortefeuilleEtpChargeDto> Charges { get; set; }
        = new();
}
