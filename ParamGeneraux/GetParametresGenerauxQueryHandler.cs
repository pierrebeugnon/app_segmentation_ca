using MapsterMapper;
using Segmentation.Application.Contracts;
using Segmentation.Application.Queries.ParametresGeneraux;
using Segmentation.Application.Responses;
using Segmentation.Core.Repositories;

namespace Segmentation.Application.Handlers.ParametresGeneraux
{
    internal class GetParametresGenerauxQueryHandler
        : IQueryHandler<GetParametresGenerauxQuery, ParametresGenerauxResponse?>
    {
        private readonly IMapper _mapper;
        private readonly IParametresGenerauxRepository _repository;

        public GetParametresGenerauxQueryHandler(
            IMapper mapper,
            IParametresGenerauxRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<ParametresGenerauxResponse?> Handle(
            GetParametresGenerauxQuery request,
            CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync();
            var first = entities.FirstOrDefault();

            return first is null
                ? null
                : _mapper.Map<ParametresGenerauxResponse>(first);
        }
    }
}
