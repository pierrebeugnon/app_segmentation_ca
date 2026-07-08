using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Segmentation.Core.Entities;

namespace Segmentation.Infrastructure.Data.Configurations
{
    public class SegmentationDistributivesConfiguration
        : IEntityTypeConfiguration<SegmentationDistributives>
    {
        public void Configure(EntityTypeBuilder<SegmentationDistributives> builder)
        {
            builder.ToTable("SegmentationDistributives");

            builder.HasKey(x => x.SegmentationDistributiveID);

            builder.Property(x => x.SegmentationDistributiveID)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.LibRegion).IsRequired().HasMaxLength(100);
            builder.Property(x => x.CodeRegion).IsRequired().HasMaxLength(20);
            builder.Property(x => x.LibSecteur).IsRequired().HasMaxLength(100);
            builder.Property(x => x.LibAgence).IsRequired().HasMaxLength(150);
            builder.Property(x => x.CodeAgence).IsRequired().HasMaxLength(20);
            builder.Property(x => x.MatriculeConseiller).IsRequired().HasMaxLength(20);
            builder.Property(x => x.TypeConseiller).IsRequired().HasMaxLength(50);

            builder.Property(x => x.HDGPremiumPotentiel).HasDefaultValue(0);
            builder.Property(x => x.HDGPremiumStandard).HasDefaultValue(0);
            builder.Property(x => x.HDGPotentiel).HasDefaultValue(0);
            builder.Property(x => x.HDGSeniorEpargnant).HasDefaultValue(0);
            builder.Property(x => x.HDGStandard).HasDefaultValue(0);

            builder.Property(x => x.CIPotentiel).HasDefaultValue(0);
            builder.Property(x => x.CIStandard).HasDefaultValue(0);

            builder.Property(x => x.GPPotentiel).HasDefaultValue(0);
            builder.Property(x => x.GPStandard).HasDefaultValue(0);

            builder.Property(x => x.NonSegmente).HasDefaultValue(0);
            builder.Property(x => x.NonClasse).HasDefaultValue(0);

            // Index utiles pour les filtres du front
            builder.HasIndex(x => new { x.CodeRegion, x.LibRegion });
            builder.HasIndex(x => x.LibSecteur);
            builder.HasIndex(x => new { x.CodeAgence, x.LibAgence });
            builder.HasIndex(x => x.MatriculeConseiller);
        }
    }
}
