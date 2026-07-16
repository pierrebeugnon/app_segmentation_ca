using MapsterMapper;
using Segmentation.Application.Contracts;
using Segmentation.Application.Queries.SegmentIntensite;
using Segmentation.Application.Responses;
using Segmentation.Core.Repositories;

namespace Segmentation.Application.Handlers.SegmentIntensite
{
    internal class GetAllSegmentIntensiteQueryHandler
        : IQueryHandler<GetAllSegmentIntensiteQuery, List<SegmentIntensiteResponse>>
    {
        private readonly IMapper _mapper;
        private readonly ISegmentsIntensitesRepository _repository;

        public GetAllSegmentIntensiteQueryHandler(
            IMapper mapper,
            ISegmentsIntensitesRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<List<SegmentIntensiteResponse>> Handle(
            GetAllSegmentIntensiteQuery request,
            CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<List<SegmentIntensiteResponse>>(entities.ToList());
        }
    }
}
