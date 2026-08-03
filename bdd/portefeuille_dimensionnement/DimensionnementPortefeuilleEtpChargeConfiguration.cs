using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Segmentation.Core.Entities;

namespace Segmentation.Infrastructure.Configurations;

public class DimensionnementPortefeuilleEtpChargeConfiguration
    : IEntityTypeConfiguration<DimensionnementPortefeuilleEtpCharge>
{
    public void Configure(EntityTypeBuilder<DimensionnementPortefeuilleEtpCharge> builder)
    {
        builder.ToTable("DimensionnementPortefeuilleEtpCharge");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LibRegion)
            .HasMaxLength(150);

        builder.Property(x => x.LibSecteur)
            .HasMaxLength(150);

        builder.Property(x => x.LibAgence)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Segment)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ChargeATransfererBP)
            .HasColumnType("decimal(10,4)");

        builder.Property(x => x.ChargeRecueBP)
            .HasColumnType("decimal(10,4)");

        builder.Property(x => x.ChargeATransfererMutualise)
            .HasColumnType("decimal(10,4)");

        builder.Property(x => x.DateMaj)
            .IsRequired();

        builder.HasIndex(x => x.LibAgence);

        builder.HasIndex(x => new
        {
            x.LibAgence,
            x.Segment
        });
    }
}
