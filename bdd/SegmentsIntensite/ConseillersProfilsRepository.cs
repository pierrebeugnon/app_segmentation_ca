using Segmentation.Core.Entities;
using Segmentation.Core.Repositories;
using Segmentation.Infrastructure.Data;

namespace Segmentation.Infrastructure.Repositories
{
    public class ConseillersProfilsRepository
        : BaseRepository<ConseillersProfils>,
          IConseillersProfilsRepository
    {
        public ConseillersProfilsRepository(SegmentationContext context)
            : base(context)
        {
        }
    }
}
