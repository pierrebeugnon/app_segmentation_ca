using MapsterMapper;
using Segmentation.Application.Contracts;
using Segmentation.Application.Queries.RegleAffectationSegment;
using Segmentation.Application.Responses;
using Segmentation.Core.Repositories;

namespace Segmentation.Application.Handlers.RegleAffectationSegment
{
    internal class GetAllRegleAffectationSegmentQueryHandler
        : IQueryHandler<GetAllRegleAffectationSegmentQuery, List<RegleAffectationSegmentResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IReglesAffectationSegmentsRepository _repository;

        public GetAllRegleAffectationSegmentQueryHandler(
            IMapper mapper,
            IReglesAffectationSegmentsRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<List<RegleAffectationSegmentResponse>> Handle(
            GetAllRegleAffectationSegmentQuery request,
            CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<List<RegleAffectationSegmentResponse>>(entities.ToList());
        }
    }
}
