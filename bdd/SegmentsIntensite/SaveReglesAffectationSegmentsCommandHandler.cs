using MediatR;
using Segmentation.Application.Commands.RegleAffectationSegment;
using Segmentation.Core.Entities;
using Segmentation.Core.Repositories;

namespace Segmentation.Application.Handlers.RegleAffectationSegment
{
    internal class SaveReglesAffectationSegmentsCommandHandler
        : IRequestHandler<SaveReglesAffectationSegmentsCommand, int>
    {
        private readonly IReglesAffectationSegmentsRepository _repository;

        public SaveReglesAffectationSegmentsCommandHandler(
            IReglesAffectationSegmentsRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(
            SaveReglesAffectationSegmentsCommand request,
            CancellationToken cancellationToken)
        {
            var existing = await _repository.GetAllAsync();
            await _repository.DeleteRangeAsync(existing, deletePhysically: true);

            var newEntities = request.Items
                .Where(x => !string.IsNullOrWhiteSpace(x.Segment))
                .Select(x => new ReglesAffectationSegments
                {
                    Segment = x.Segment ?? "",
                    ConseillerPrioritaire = string.IsNullOrWhiteSpace(x.ConseillerPrioritaire) ? null : x.ConseillerPrioritaire,
                    ConseillerSecondaire = string.IsNullOrWhiteSpace(x.ConseillerSecondaire) ? null : x.ConseillerSecondaire,
                    ConseillerTertiaire = string.IsNullOrWhiteSpace(x.ConseillerTertiaire) ? null : x.ConseillerTertiaire
                })
                .ToList();

            await _repository.AddRangeAsync(newEntities);

            return newEntities.Count;
        }
    }
}
