using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Segmentation.Application.Commands.RegleAffectationSegment;
using Segmentation.Application.Queries.RegleAffectationSegment;
using Segmentation.Shared.Models;

namespace Segmentation.Server.Controllers
{
    public class RegleAffectationSegmentController : SegmentationBaseController
    {
        public RegleAffectationSegmentController(IMediator mediator, IMapper mapper)
            : base(mediator, mapper)
        {
        }

        // ── GET /api/RegleAffectationSegment ────────────────────────
        [HttpGet(Name = "GetAllRegleAffectationSegment")]
        public async Task<List<RegleAffectationSegmentData>> GetAll()
        {
            var query = new GetAllRegleAffectationSegmentQuery();

            return _mapper.Map<List<RegleAffectationSegmentData>>(
                await _mediator.Send(query));
        }

        // ── POST /api/RegleAffectationSegment ───────────────────────
        [HttpPost(Name = "SaveReglesAffectationSegments")]
        public async Task<IActionResult> Save(
            [FromBody] List<RegleAffectationSegmentData> items)
        {
            if (items is null)
                return BadRequest("Liste vide.");

            var command = new SaveReglesAffectationSegmentsCommand
            {
                Items = items.Select(x => new SaveRegleAffectationSegmentItem
                {
                    Segment = x.Segment,
                    ConseillerPrioritaire = x.ConseillerPrioritaire,
                    ConseillerSecondaire = x.ConseillerSecondaire,
                    ConseillerTertiaire = x.ConseillerTertiaire
                }).ToList()
            };

            var count = await _mediator.Send(command);

            return Ok(new { saved = count });
        }
    }
}
