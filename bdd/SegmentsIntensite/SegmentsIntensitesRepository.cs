using Segmentation.Core.Entities;
using Segmentation.Core.Repositories;
using Segmentation.Infrastructure.Data;

namespace Segmentation.Infrastructure.Repositories
{
    public class SegmentsIntensitesRepository
        : BaseRepository<SegmentsIntensite>,
          ISegmentsIntensitesRepository
    {
        public SegmentsIntensitesRepository(SegmentationContext context)
            : base(context)
        {
        }
    }
}
