using d_lama_service.Repositories.Core;
using Data;
using Data.ProjectEntities;

namespace d_lama_service.Repositories.ProjectRepositories
{
    public class DataPointRepository : Repository<DataPoint>, IDataPointRepository
    {
        /// <summary>
        /// Constructor of DataPointRepository.
        /// </summary>
        /// <param name="context"> The DB context. </param>
        public DataPointRepository(DataContext context) : base(context) { }
    }
}
