using MediatR;
using Microsoft.AspNetCore.Mvc;
using Segmentation.Application.Commands.DimensionnementPortefeuille;
using Segmentation.Shared.Models.DimensionnementPortefeuille;

namespace Segmentation.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DimensionnementPortefeuilleEtpController : ControllerBase
{
    private readonly IMediator _mediator;

    public DimensionnementPortefeuilleEtpController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Sauvegarde du dimensionnement ETP d'une agence.
    /// La sauvegarde écrase les données déjà présentes pour l'agence.
    /// </summary>
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

        var result = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(result);
    }
}
