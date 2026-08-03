using Microsoft.AspNetCore.Mvc;
using Segmentation.Application.Commands.DimensionnementPortefeuille;
using Segmentation.Application.Contracts;
using Segmentation.Shared.Models.DimensionnementPortefeuille;

namespace Segmentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DimensionnementPortefeuilleEtpController : ControllerBase
{
    private readonly ICommandDispatcher _dispatcher;

    public DimensionnementPortefeuilleEtpController(ICommandDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    public async Task<ActionResult<bool>> Save(
        [FromBody] SaveDimensionnementPortefeuilleEtpRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest("La requête est vide.");

        if (string.IsNullOrWhiteSpace(request.LibAgence))
            return BadRequest("L'agence est obligatoire.");

        var command = new SaveDimensionnementPortefeuilleEtpCommand(request);

        var result = await _dispatcher.Dispatch<
            SaveDimensionnementPortefeuilleEtpCommand,
            bool>(command, cancellationToken);

        return Ok(result);
    }
}
