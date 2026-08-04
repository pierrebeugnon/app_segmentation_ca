using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Segmentation.Application.Commands.ConseillerProfil;
using Segmentation.Application.Queries.ConseillerProfil;
using Segmentation.Shared.Models;

namespace Segmentation.Server.Controllers
{
    public class ConseillerProfilController : SegmentationBaseController
    {
        public ConseillerProfilController(IMediator mediator, IMapper mapper)
            : base(mediator, mapper)
        {
        }

        // ── GET /api/ConseillerProfil ──────────────────────────────
        [HttpGet(Name = "GetAllConseillerProfil")]
        public async Task<List<ConseillerProfilData>> GetAll()
        {
            var query = new GetAllConseillerProfilQuery();

            return _mapper.Map<List<ConseillerProfilData>>(
                await _mediator.Send(query));
        }

        // ── POST /api/ConseillerProfil ─────────────────────────────
        [HttpPost(Name = "SaveConseillersProfils")]
        public async Task<IActionResult> Save(
            [FromBody] List<ConseillerProfilData> items)
        {
            if (items is null)
                return BadRequest("Liste vide.");

            var command = new SaveConseillersProfilsCommand
            {
                Items = items.Select(x => new SaveConseillerProfilItem
                {
                    LigneMetier = x.LigneMetier,
                    Profil = x.Profil,
                    PartTempsCommercialPct = x.PartTempsCommercialPct
                }).ToList()
            };

            var count = await _mediator.Send(command);

            return Ok(new { saved = count });
        }
    }
}
