using Segmentation.Core.Entities;
using Segmentation.Core.Repositories;
using Segmentation.Infrastructure.Data;

namespace Segmentation.Infrastructure.Repositories
{
    public class ParametresGenerauxRepository
        : BaseRepository<ParametresGeneraux>,
          IParametresGenerauxRepository
    {
        public ParametresGenerauxRepository(SegmentationContext context)
            : base(context)
        {
        }
    }
}
