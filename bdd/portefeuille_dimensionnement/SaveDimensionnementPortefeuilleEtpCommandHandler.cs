using MediatR;
using Segmentation.Application.Commands.DimensionnementPortefeuille;
using Segmentation.Core.Entities;
using Segmentation.Core.Repositories;

namespace Segmentation.Application.Handlers.DimensionnementPortefeuille
{
	internal class SaveDimensionnementPortefeuilleEtpCommandHandler
		: IRequestHandler<SaveDimensionnementPortefeuilleEtpCommand, bool>
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
			if (command == null)
			{
				throw new ArgumentNullException(
					nameof(command),
					"La commande de sauvegarde est vide.");
			}

			if (string.IsNullOrWhiteSpace(command.LibAgence))
			{
				throw new ArgumentException(
					"L'agence est obligatoire pour enregistrer le dimensionnement.",
					nameof(command.LibAgence));
			}

			var libRegion = command.LibRegion?.Trim();
			var libSecteur = command.LibSecteur?.Trim();
			var libAgence = command.LibAgence.Trim();

			var now = DateTime.UtcNow;

			var lignes = (command.Lignes ?? new())
				.Select(l => new DimensionnementPortefeuilleEtp
				{
					LibRegion = libRegion,
					LibSecteur = libSecteur,
					LibAgence = libAgence,

					Segment = l.Segment?.Trim() ?? string.Empty,

					ProfilConseiller =
						l.ProfilConseiller?.Trim() ?? string.Empty,

					MatriculeConseiller =
						string.IsNullOrWhiteSpace(l.MatriculeConseiller)
							? null
							: l.MatriculeConseiller.Trim(),

					SlotLabel =
						l.SlotLabel?.Trim() ?? string.Empty,

					IsActuel = l.IsActuel,
					IsCible = l.IsCible,

					EtpExistant = l.EtpExistant,
					EtpCible = l.EtpCible,

					CapaciteEtpActuel = l.CapaciteEtpActuel,
					CapaciteEtpCible = l.CapaciteEtpCible,

					DateMaj = now
				})
				.ToList();

			var charges = (command.Charges ?? new())
				.Select(c => new DimensionnementPortefeuilleEtpCharge
				{
					LibRegion = libRegion,
					LibSecteur = libSecteur,
					LibAgence = libAgence,

					Segment = c.Segment?.Trim() ?? string.Empty,

					ChargeATransfererBP =
						c.ChargeATransfererBP,

					ChargeRecueBP =
						c.ChargeRecueBP,

					ChargeATransfererMutualise =
						c.ChargeATransfererMutualise,

					DateMaj = now
				})
				.ToList();

			await _repository.SaveAsync(
				libAgence,
				lignes,
				charges,
				cancellationToken);

			return true;
		}
	}
}
