using Segmentation.Core.Entities;
using Segmentation.Core.Repositories;
using Segmentation.Infrastructure.Data;

namespace Segmentation.Infrastructure.Repositories
{
    public class ReglesAffectationSegmentsRepository
        : BaseRepository<ReglesAffectationSegments>,
          IReglesAffectationSegmentsRepository
    {
        public ReglesAffectationSegmentsRepository(SegmentationContext context)
            : base(context)
        {
        }
    }
}
