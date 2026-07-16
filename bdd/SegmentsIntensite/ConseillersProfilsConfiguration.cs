using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Segmentation.Core.Entities;

namespace Segmentation.Infrastructure.Data.Configurations
{
    public class ConseillersProfilsConfiguration
        : IEntityTypeConfiguration<ConseillersProfils>
    {
        public void Configure(EntityTypeBuilder<ConseillersProfils> builder)
        {
            builder.ToTable("ConseillersProfils");

            builder.HasKey(x => x.ConseillerProfilID);

            builder.Property(x => x.ConseillerProfilID)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.LigneMetier).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Profil).IsRequired().HasMaxLength(100);
        }
    }
}
