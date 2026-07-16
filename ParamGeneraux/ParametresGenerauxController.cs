using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Segmentation.Application.Commands.ParametresGeneraux;
using Segmentation.Application.Queries.ParametresGeneraux;
using Segmentation.Shared.Models;

namespace Segmentation.Server.Controllers
{
    public class ParametresGenerauxController : SegmentationBaseController
    {
        public ParametresGenerauxController(IMediator mediator, IMapper mapper)
            : base(mediator, mapper)
        {
        }

        // ── GET /api/ParametresGeneraux ─────────────────────────────
        [HttpGet(Name = "GetParametresGeneraux")]
        public async Task<ParametresGenerauxData?> Get()
        {
            var query = new GetParametresGenerauxQuery();
            var response = await _mediator.Send(query);

            return response is null
                ? null
                : _mapper.Map<ParametresGenerauxData>(response);
        }

        // ── POST /api/ParametresGeneraux ────────────────────────────
        [HttpPost(Name = "SaveParametresGeneraux")]
        public async Task<IActionResult> Save(
            [FromBody] ParametresGenerauxData data)
        {
            if (data is null)
                return BadRequest("Données vides.");

            var command = new SaveParametresGenerauxCommand
            {
                HeuresParSemaine = data.HeuresParSemaine,
                NbSemainesParAn = data.NbSemainesParAn
            };

            var id = await _mediator.Send(command);

            return Ok(new { id });
        }
    }
}
