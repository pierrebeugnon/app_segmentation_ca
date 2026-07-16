using MediatR;
using Segmentation.Application.Commands.ConseillerProfil;
using Segmentation.Core.Entities;
using Segmentation.Core.Repositories;

namespace Segmentation.Application.Handlers.ConseillerProfil
{
    internal class SaveConseillersProfilsCommandHandler
        : IRequestHandler<SaveConseillersProfilsCommand, int>
    {
        private readonly IConseillersProfilsRepository _repository;

        public SaveConseillersProfilsCommandHandler(
            IConseillersProfilsRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(
            SaveConseillersProfilsCommand request,
            CancellationToken cancellationToken)
        {
            var existing = await _repository.GetAllAsync();
            await _repository.DeleteRangeAsync(existing, deletePhysically: true);

            var newEntities = request.Items
                .Where(x => !string.IsNullOrWhiteSpace(x.Profil))
                .Select(x => new ConseillersProfils
                {
                    LigneMetier = x.LigneMetier ?? "",
                    Profil = x.Profil ?? "",
                    PartTempsCommercialPct = x.PartTempsCommercialPct
                })
                .ToList();

            await _repository.AddRangeAsync(newEntities);

            return newEntities.Count;
        }
    }
}
