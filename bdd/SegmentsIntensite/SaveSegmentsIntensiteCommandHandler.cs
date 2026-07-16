using MediatR;
using Segmentation.Application.Commands.SegmentIntensite;
using Segmentation.Core.Entities;
using Segmentation.Core.Repositories;

namespace Segmentation.Application.Handlers.SegmentIntensite
{
    internal class SaveSegmentsIntensiteCommandHandler
        : IRequestHandler<SaveSegmentsIntensiteCommand, int>
    {
        private readonly ISegmentsIntensitesRepository _repository;

        public SaveSegmentsIntensiteCommandHandler(
            ISegmentsIntensitesRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(
            SaveSegmentsIntensiteCommand request,
            CancellationToken cancellationToken)
        {
            var existing = await _repository.GetAllAsync();
            await _repository.DeleteRangeAsync(existing, deletePhysically: true);

            var newEntities = request.Items
                .Where(x => !string.IsNullOrWhiteSpace(x.Segment))
                .Select(x => new SegmentsIntensite
                {
                    LigneMetier = x.LigneMetier ?? "",
                    Segment = x.Segment ?? "",
                    NombreRdvParAn = x.NombreRdvParAn,
                    DureeRdvHeures = x.DureeRdvHeures
                })
                .ToList();

            await _repository.AddRangeAsync(newEntities);

            return newEntities.Count;
        }
    }
}
