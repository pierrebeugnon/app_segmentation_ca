using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Segmentation.Application.Commands.SegmentationDistributive;
using Segmentation.Core.Entities;
using Segmentation.Core.Repositories;

namespace Segmentation.Application.Handlers.SegmentationDistributive
{
    public class ImportSegmentationDistributiveCommandHandler
        : IRequestHandler<ImportSegmentationDistributiveCommand, int>
    {
        private readonly ISegmentationDistributivesRepository _repository;

        public ImportSegmentationDistributiveCommandHandler(
            ISegmentationDistributivesRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(
            ImportSegmentationDistributiveCommand request,
            CancellationToken cancellationToken)
        {
            var config = new CsvConfiguration(CultureInfo.GetCultureInfo("fr-FR"))
            {
                Delimiter = ";",
                HeaderValidated = null,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = null
            };

            using var reader = new StreamReader(request.CsvStream);
            using var csv = new CsvReader(reader, config);

            var rows = csv.GetRecords<SegmentationDistributiveCsvRow>().ToList();

            var entities = rows
                .Where(r =>
                    !string.IsNullOrWhiteSpace(r.LibRegion) &&
                    !r.LibAgence.ToUpper().Contains("TOTAL") &&
                    !r.LibSecteur.ToUpper().Contains("TOTAL") &&
                    !r.LibRegion.ToUpper().Contains("TOTAL"))
                .Select(r => new SegmentationDistributives
                {
                    LibRegion = r.LibRegion,
                    CodeRegion = r.CodeRegion,
                    LibSecteur = r.LibSecteur,
                    LibAgence = r.LibAgence,
                    CodeAgence = r.CodeAgence,
                    MatriculeConseiller = r.MatriculeConseiller,
                    TypeConseiller = r.TypeConseiller,
                    HDGPremiumPotentiel = r.HDGPremiumPotentiel,
                    HDGPremiumStandard  = r.HDGPremiumStandard,
                    HDGPotentiel        = r.HDGPotentiel,
                    HDGSeniorEpargnant  = r.HDGSeniorEpargnant,
                    HDGStandard         = r.HDGStandard,
                    CIPotentiel         = r.CIPotentiel,
                    CIStandard          = r.CIStandard,
                    GPPotentiel         = r.GPPotentiel,
                    GPStandard          = r.GPStandard,
                    NonSegmente         = r.NonSegmente,
                    NonClasse           = r.NonClasse
                })
                .ToList();

            if (request.ReplaceExisting)
            {
                var existing = await _repository.GetAllAsync();
                await _repository.DeleteRangeAsync(existing, deletePhysically: true);
            }

            await _repository.AddRangeAsync(entities);

            return entities.Count;
        }
    }
}
