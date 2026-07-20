using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Segmentation.Application.Commands.SegmentationDistributive;
using Segmentation.Application.Queries.SegmentationDistributive;
using Segmentation.Shared.Models;

namespace Segmentation.Server.Controllers
{
    public class SegmentationDistributiveController : SegmentationBaseController
    {
        public SegmentationDistributiveController(IMediator mediator, IMapper mapper)
            : base(mediator, mapper)
        {
        }

        // ── GET /api/SegmentationDistributive ──
        [HttpGet(Name = "GetAllSegmentationDistributive")]
        public async Task<List<SegmentationDistributiveData>> GetAll()
        {
            var query = new GetAllSegmentationDistributiveQuery();

            return _mapper.Map<List<SegmentationDistributiveData>>(
                await _mediator.Send(query));
        }

        // ── GET /api/SegmentationDistributive/id/{id} ──
        [HttpGet(template: "id/{id}", Name = "GetSegmentationDistributiveById")]
        public async Task<SegmentationDistributiveData> GetById(int id)
        {
            var query = new GetSegmentationDistributiveByIdQuery
            {
                Id = id
            };

            return _mapper.Map<SegmentationDistributiveData>(
                await _mediator.Send(query));
        }

        // ── GET /api/SegmentationDistributive/referentiel ──
        [HttpGet(template: "referentiel", Name = "GetReferentiel")]
        public async Task<ReferentielData> GetReferentiel()
        {
            var response = await _mediator.Send(new GetReferentielQuery());

            return new ReferentielData
            {
                Segments = response.Segments,
                Profils = response.Profils.Select(p => new ReferentielProfilData
                {
                    Profil = p.Profil,
                    LigneMetier = p.LigneMetier
                }).ToList(),
                Regions = response.Regions,
                Secteurs = response.Secteurs,
                Agences = response.Agences
            };
        }

        // ── POST /api/SegmentationDistributive/import ──
        [HttpPost(template: "import", Name = "ImportSegmentationDistributive")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> Import(
            IFormFile file,
            [FromQuery] bool replaceExisting = false)
        {
            if (file is null || file.Length == 0)
                return BadRequest("Fichier vide.");

            using Stream stream = file.OpenReadStream();

            int count = await _mediator.Send(new ImportSegmentationDistributiveCommand
            {
                CsvStream = stream,
                ReplaceExisting = replaceExisting
            });

            return Ok(new { imported = count });
        }
    }
}
