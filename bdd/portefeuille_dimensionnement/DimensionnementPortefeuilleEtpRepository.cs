using Microsoft.EntityFrameworkCore;
using Segmentation.Core.Entities;
using Segmentation.Core.Repositories;

namespace Segmentation.Infrastructure.Repositories;

public class DimensionnementPortefeuilleEtpRepository
    : IDimensionnementPortefeuilleEtpRepository
{
    private readonly SegmentationDbContext _context;

    public DimensionnementPortefeuilleEtpRepository(SegmentationDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(
        string agence,
        List<DimensionnementPortefeuilleEtp> lignes,
        List<DimensionnementPortefeuilleEtpCharge> charges,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(agence))
            throw new ArgumentException("L'agence est obligatoire.", nameof(agence));

        var agenceTrim = agence.Trim();

        var lignesExistantes = await _context
            .Set<DimensionnementPortefeuilleEtp>()
            .Where(x => x.LibAgence == agenceTrim)
            .ToListAsync(cancellationToken);

        var chargesExistantes = await _context
            .Set<DimensionnementPortefeuilleEtpCharge>()
            .Where(x => x.LibAgence == agenceTrim)
            .ToListAsync(cancellationToken);

        if (lignesExistantes.Any())
            _context.Set<DimensionnementPortefeuilleEtp>().RemoveRange(lignesExistantes);

        if (chargesExistantes.Any())
            _context.Set<DimensionnementPortefeuilleEtpCharge>().RemoveRange(chargesExistantes);

        if (lignes.Any())
            await _context.Set<DimensionnementPortefeuilleEtp>()
                .AddRangeAsync(lignes, cancellationToken);

        if (charges.Any())
            await _context.Set<DimensionnementPortefeuilleEtpCharge>()
                .AddRangeAsync(charges, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<DimensionnementPortefeuilleEtp>> GetByAgenceAsync(
        string agence,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(agence))
            return new List<DimensionnementPortefeuilleEtp>();

        var agenceTrim = agence.Trim();

        return await _context
            .Set<DimensionnementPortefeuilleEtp>()
            .AsNoTracking()
            .Where(x => x.LibAgence == agenceTrim)
            .OrderBy(x => x.Segment)
            .ThenBy(x => x.ProfilConseiller)
            .ThenBy(x => x.SlotLabel)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DimensionnementPortefeuilleEtpCharge>> GetChargesAsync(
        string agence,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(agence))
            return new List<DimensionnementPortefeuilleEtpCharge>();

        var agenceTrim = agence.Trim();

        return await _context
            .Set<DimensionnementPortefeuilleEtpCharge>()
            .AsNoTracking()
            .Where(x => x.LibAgence == agenceTrim)
            .OrderBy(x => x.Segment)
            .ToListAsync(cancellationToken);
    }
}
