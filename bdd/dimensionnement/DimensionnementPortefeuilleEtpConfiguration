using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Segmentation.Core.Entities;

namespace Segmentation.Infrastructure.Configurations;

public class DimensionnementPortefeuilleEtpConfiguration
    : IEntityTypeConfiguration<DimensionnementPortefeuilleEtp>
{
    public void Configure(EntityTypeBuilder<DimensionnementPortefeuilleEtp> builder)
    {
        builder.ToTable("DimensionnementPortefeuilleEtp");

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

        builder.Property(x => x.ProfilConseiller)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.MatriculeConseiller)
            .HasMaxLength(50);

        builder.Property(x => x.SlotLabel)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EtpExistant)
            .HasColumnType("decimal(10,4)");

        builder.Property(x => x.EtpCible)
            .HasColumnType("decimal(10,4)");

        builder.Property(x => x.CapaciteEtpActuel)
            .HasColumnType("decimal(10,4)");

        builder.Property(x => x.CapaciteEtpCible)
            .HasColumnType("decimal(10,4)");

        builder.Property(x => x.DateMaj)
            .IsRequired();

        builder.HasIndex(x => x.LibAgence);

        builder.HasIndex(x => new
        {
            x.LibAgence,
            x.Segment,
            x.MatriculeConseiller,
            x.ProfilConseiller
        });
    }
}
