using Segmentation.Application.Contracts;
using Segmentation.Shared.Models.DimensionnementPortefeuille;

namespace Segmentation.Application.Commands.DimensionnementPortefeuille;

public class SaveDimensionnementPortefeuilleEtpCommand
    : ICommand<bool>
{
    public SaveDimensionnementPortefeuilleEtpRequest Request { get; }

    public SaveDimensionnementPortefeuilleEtpCommand(
        SaveDimensionnementPortefeuilleEtpRequest request)
    {
        Request = request;
    }
}
