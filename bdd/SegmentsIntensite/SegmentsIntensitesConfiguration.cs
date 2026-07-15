using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Segmentation.Core.Entities;

namespace Segmentation.Infrastructure.Data.Configurations
{
    public class SegmentsIntensitesConfiguration
        : IEntityTypeConfiguration<SegmentsIntensite>
    {
        public void Configure(EntityTypeBuilder<SegmentsIntensite> builder)
        {
            builder.ToTable("SegmentsIntensite");

            builder.HasKey(x => x.SegmentIntensiteID);

            builder.Property(x => x.SegmentIntensiteID)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.LigneMetier).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Segment).IsRequired().HasMaxLength(100);

            builder.HasIndex(x => x.Segment).IsUnique();
        }
    }
}
