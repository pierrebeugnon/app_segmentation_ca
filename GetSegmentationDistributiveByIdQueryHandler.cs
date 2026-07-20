using MapsterMapper;
using Segmentation.Application.Contracts;
using Segmentation.Application.Queries.SegmentationDistributive;
using Segmentation.Application.Responses;
using Segmentation.Core.Entities;
using Segmentation.Core.Repositories;

namespace Segmentation.Application.Handlers.SegmentationDistributive
{
    internal class GetSegmentationDistributiveByIdQueryHandler
        : IQueryHandler<GetSegmentationDistributiveByIdQuery, SegmentationDistributiveResponse?>
    {
        private readonly IMapper _mapper;
        private readonly ISegmentationDistributivesRepository _repository;

        public GetSegmentationDistributiveByIdQueryHandler(
            IMapper mapper,
            ISegmentationDistributivesRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<SegmentationDistributiveResponse?> Handle(
            GetSegmentationDistributiveByIdQuery request,
            CancellationToken cancellationToken)
        {
            SegmentationDistributives? entity = await _repository.GetByIdAsync(request.Id);

            return entity is null
                ? null
                : _mapper.Map<SegmentationDistributiveResponse>(entity);
        }
    }
}
