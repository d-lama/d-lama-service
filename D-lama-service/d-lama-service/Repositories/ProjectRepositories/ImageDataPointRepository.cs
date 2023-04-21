using d_lama_service.Repositories.Core;
using Data;
using Data.ProjectEntities;

namespace d_lama_service.Repositories.ProjectRepositories
{
    /// <summary>
    /// ImageDataPointRepository Class.
    /// </summary>
    public class ImageDataPointRepository : Repository<ImageDataPoint>, IImageDataPointRepository
    {
        /// <summary>
        /// Constructor of TextDataPointRepository.
        /// </summary>
        /// <param name="context"> The DB context. </param>
        public ImageDataPointRepository(DataContext context) : base(context) { }
    }
}
