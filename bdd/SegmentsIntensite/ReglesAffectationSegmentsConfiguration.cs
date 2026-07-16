using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Segmentation.Core.Entities;

namespace Segmentation.Infrastructure.Data.Configurations
{
    public class ReglesAffectationSegmentsConfiguration
        : IEntityTypeConfiguration<ReglesAffectationSegments>
    {
        public void Configure(EntityTypeBuilder<ReglesAffectationSegments> builder)
        {
            builder.ToTable("ReglesAffectationSegments");

            builder.HasKey(x => x.RegleAffectationID);

            builder.Property(x => x.RegleAffectationID)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Segment).IsRequired().HasMaxLength(100);
            builder.Property(x => x.ConseillerPrioritaire).HasMaxLength(100);
            builder.Property(x => x.ConseillerSecondaire).HasMaxLength(100);
            builder.Property(x => x.ConseillerTertiaire).HasMaxLength(100);

            builder.HasIndex(x => x.Segment).IsUnique();
        }
    }
}
