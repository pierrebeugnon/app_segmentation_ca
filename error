using Segmentation.Core.Entities;

namespace Segmentation.Core.Repositories;

public interface IDimensionnementPortefeuilleEtpRepository
{
    Task SaveAsync(
        string agence,
        List<DimensionnementPortefeuilleEtp> lignes,
        List<DimensionnementPortefeuilleEtpCharge> charges,
        CancellationToken cancellationToken = default);

    Task<List<DimensionnementPortefeuilleEtp>> GetByAgenceAsync(
        string agence,
        CancellationToken cancellationToken = default);

    Task<List<DimensionnementPortefeuilleEtpCharge>> GetChargesAsync(
        string agence,
        CancellationToken cancellationToken = default);
}
