using MediatR;
using Segmentation.Shared.Models;

namespace Segmentation.Application.Commands.DimensionnementPortefeuille;

public class SaveDimensionnementPortefeuilleEtpCommand
    : ICommand<bool>
{
    public SaveDimensionnementPortefeuilleEtpData Data { get; set; }
        = new();
}
