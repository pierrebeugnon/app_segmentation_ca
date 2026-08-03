using Segmentation.Application.Commands.DimensionnementPortefeuille;
using Segmentation.Application.Contracts;
using Segmentation.Core.Entities;
using Segmentation.Core.Repositories;

namespace Segmentation.Application.Handlers.DimensionnementPortefeuille;

internal class SaveDimensionnementPortefeuilleEtpCommandHandler
    : ICommandHandler<SaveDimensionnementPortefeuilleEtpCommand, bool>
{
    private readonly IDimensionnementPortefeuilleEtpRepository _repository;

    public SaveDimensionnementPortefeuilleEtpCommandHandler(
        IDimensionnementPortefeuilleEtpRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        SaveDimensionnementPortefeuilleEtpCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        if (request == null)
            throw new ArgumentException("La requête de sauvegarde est vide.");

        if (string.IsNullOrWhiteSpace(request.LibAgence))
            throw new ArgumentException("L'agence est obligatoire pour enregistrer le dimensionnement.");

        var now = DateTime.UtcNow;

        var lignes = request.Lignes
            .Select(l => new DimensionnementPortefeuilleEtp
            {
                LibRegion = request.LibRegion?.Trim(),
                LibSecteur = request.LibSecteur?.Trim(),
                LibAgence = request.LibAgence.Trim(),

                Segment = l.Segment?.Trim() ?? string.Empty,

                ProfilConseiller = l.ProfilConseiller?.Trim() ?? string.Empty,
                MatriculeConseiller = l.MatriculeConseiller?.Trim(),
                SlotLabel = l.SlotLabel?.Trim() ?? string.Empty,

                IsActuel = l.IsActuel,
                IsCible = l.IsCible,

                EtpExistant = l.EtpExistant,
                EtpCible = l.EtpCible,

                CapaciteEtpActuel = l.CapaciteEtpActuel,
                CapaciteEtpCible = l.CapaciteEtpCible,

                DateMaj = now
            })
            .ToList();

        var charges = request.Charges
            .Select(c => new DimensionnementPortefeuilleEtpCharge
            {
                LibRegion = request.LibRegion?.Trim(),
                LibSecteur = request.LibSecteur?.Trim(),
                LibAgence = request.LibAgence.Trim(),

                Segment = c.Segment?.Trim() ?? string.Empty,

                ChargeATransfererBP = c.ChargeATransfererBP,
                ChargeRecueBP = c.ChargeRecueBP,
                ChargeATransfererMutualise = c.ChargeATransfererMutualise,

                DateMaj = now
            })
            .ToList();

        await _repository.SaveAsync(
            request.LibAgence.Trim(),
            lignes,
            charges,
            cancellationToken);

        return true;
    }
}
``
