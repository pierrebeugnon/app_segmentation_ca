using MediatR;
using Segmentation.Application.Commands.ParametresGeneraux;
using Segmentation.Core.Entities;
using Segmentation.Core.Repositories;

namespace Segmentation.Application.Handlers.ParametresGeneraux
{
    internal class SaveParametresGenerauxCommandHandler
        : IRequestHandler<SaveParametresGenerauxCommand, int>
    {
        private readonly IParametresGenerauxRepository _repository;

        public SaveParametresGenerauxCommandHandler(
            IParametresGenerauxRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(
            SaveParametresGenerauxCommand request,
            CancellationToken cancellationToken)
        {
            // Upsert : on récupère l'existant, on met à jour ou on crée
            var existing = await _repository.GetAllAsync();
            var first = existing.FirstOrDefault();

            if (first is null)
            {
                // Création
                var newEntity = new ParametresGeneraux
                {
                    HeuresParSemaine = request.HeuresParSemaine,
                    NbSemainesParAn = request.NbSemainesParAn
                };

                await _repository.AddAsync(newEntity);
                return newEntity.ParametresGenerauxID;
            }
            else
            {
                // Mise à jour
                first.HeuresParSemaine = request.HeuresParSemaine;
                first.NbSemainesParAn = request.NbSemainesParAn;

                await _repository.UpdateAsync(first);
                return first.ParametresGenerauxID;
            }
        }
    }
}
