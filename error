using MapsterMapper;
using Segmentation.Application.Contracts;
using Segmentation.Application.Queries.ConseillerProfil;
using Segmentation.Application.Responses;
using Segmentation.Core.Repositories;

namespace Segmentation.Application.Handlers.ConseillerProfil
{
    internal class GetAllConseillerProfilQueryHandler
        : IQueryHandler<GetAllConseillerProfilQuery, List<ConseillerProfilResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IConseillersProfilsRepository _repository;

        public GetAllConseillerProfilQueryHandler(
            IMapper mapper,
            IConseillersProfilsRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<List<ConseillerProfilResponse>> Handle(
            GetAllConseillerProfilQuery request,
            CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<List<ConseillerProfilResponse>>(entities.ToList());
        }
    }
}
