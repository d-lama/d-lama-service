using d_lama_service.Repositories.ProjectRepositories;
using Data;

namespace d_lama_service.Services
{
    /// <summary>
    /// Implementation of the IDataPointService interface.
    /// The data point service handles the domain specific logic regarding the data points.
    /// </summary>
    public class DataPointService : Service, IDataPointService
    {
        private readonly ITextDataPointRepository _textDataPointRepository;
        private readonly IImageDataPointRepository _imageDataPointRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IDataPointRepository _dataPointRepository; 

        /// <summary>
        /// Constructor of the DataPointService.
        /// </summary>
        /// <param name="context"> The database context. </param>
        public DataPointService(DataContext context) : base(context)
        {
        }
    }
}
