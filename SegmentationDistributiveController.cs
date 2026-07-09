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

        [HttpGet(Name = "GetAllSegmentationDistributive")]
        public async Task<List<SegmentationDistributiveData>> GetAll()
        {
        var query = new GetAllSegmentationDistributiveQuery();

        return _mapper.Map<List<SegmentationDistributiveData>>
            (
        await _mediator.Send(query)
            );
        }

        [HttpGet(template: "id/{id}", Name = "GetSegmentationDistributiveById")]
        public async Task<SegmentationDistributiveData> GetById(int id)
        {
            GetSegmentationDistributiveByIdQuery query = new GetSegmentationDistributiveByIdQuery
            {
                Id = id
            };

            SegmentationDistributiveData result =
                _mapper.Map<SegmentationDistributiveData>(await _mediator.Send(query));

            return result;
        }

        [HttpGet(Name = "GetAllSegmentationDistributive")]
        public async Task<List<SegmentationDistributiveData>> GetAll()
        {
            GetAllSegmentationDistributiveQuery query = new GetAllSegmentationDistributiveQuery();

            List<SegmentationDistributiveData> result =
                _mapper.Map<List<SegmentationDistributiveData>>(await _mediator.Send(query));

            return result;
        }

        [HttpPost(template: "import", Name = "ImportSegmentationDistributive")]
        [RequestSizeLimit(50_000_000)] // 50 Mo
        public async Task<IActionResult> Import(IFormFile file, [FromQuery] bool replaceExisting = false)
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
