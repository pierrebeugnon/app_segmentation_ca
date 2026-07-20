using Segmentation.Application.Contracts;
using Segmentation.Application.Queries.SegmentationDistributive;
using Segmentation.Application.Responses;
using Segmentation.Core.Repositories;

namespace Segmentation.Application.Handlers.SegmentationDistributive
{
    internal class GetReferentielQueryHandler
        : IQueryHandler<GetReferentielQuery, ReferentielResponse>
    {
        private readonly ISegmentationDistributivesRepository _repository;

        // Mapping profil → ligne métier (Option A)
        private static readonly Dictionary<string, string> _mappingProfilLigneMetier =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["DIR. BP"]                = "banque privée",
                ["BANQUIER PRIVÉ"]         = "banque privée",
                ["CGP"]                    = "banque privée",
                ["RESP. AGENCE"]           = "retail",
                ["RCP"]                    = "retail",
                ["CONSEILLER CLIENTELE"]   = "retail",
                ["CONSEILLER COMMERCIAL"]  = "retail",
                ["CONSEILLER D'ACCUEIL"]   = "retail",
            };

        // Segments (colonnes de la table)
        private static readonly List<string> _segmentsStandards = new()
        {
            "HDG Premium Potentiel",
            "HDG Premium Standard",
            "HDG Potentiel",
            "HDG Senior Epargnant",
            "HDG Standard",
            "CI Potentiel",
            "CI Standard",
            "GP Potentiel",
            "GP Standard",
            "Non segmenté",
            "Non classé"
        };

        public GetReferentielQueryHandler(
            ISegmentationDistributivesRepository repository)
        {
            _repository = repository;
        }

        public async Task<ReferentielResponse> Handle(
            GetReferentielQuery request,
            CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync();
            var data = entities.ToList();

            // Profils uniques présents dans la table
            var profils = data
                .Where(x => !string.IsNullOrWhiteSpace(x.TypeConseiller))
                .Select(x => x.TypeConseiller.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .Select(profil => new ReferentielProfilResponse
                {
                    Profil = profil,
                    LigneMetier = _mappingProfilLigneMetier.TryGetValue(profil, out var lm)
                        ? lm
                        : "retail" // par défaut retail si profil inconnu
                })
                .ToList();

            var regions = data
                .Where(x => !string.IsNullOrWhiteSpace(x.LibRegion))
                .Select(x => x.LibRegion.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var secteurs = data
                .Where(x => !string.IsNullOrWhiteSpace(x.LibSecteur))
                .Select(x => x.LibSecteur.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var agences = data
                .Where(x => !string.IsNullOrWhiteSpace(x.LibAgence))
                .Select(x => x.LibAgence.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            return new ReferentielResponse
            {
                Segments = _segmentsStandards,
                Profils = profils,
                Regions = regions,
                Secteurs = secteurs,
                Agences = agences
            };
        }
    }
}
