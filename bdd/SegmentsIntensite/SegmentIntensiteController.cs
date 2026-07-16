using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Segmentation.Application.Queries.SegmentIntensite;
using Segmentation.Shared.Models;

namespace Segmentation.Server.Controllers
{
    public class SegmentIntensiteController : SegmentationBaseController
    {
        public SegmentIntensiteController(IMediator mediator, IMapper mapper)
            : base(mediator, mapper)
        {
        }

        [HttpGet(Name = "GetAllSegmentIntensite")]
        public async Task<List<SegmentIntensiteData>> GetAll()
        {
            var query = new GetAllSegmentIntensiteQuery();

            return _mapper.Map<List<SegmentIntensiteData>>(
                await _mediator.Send(query));
        }
    }
}
