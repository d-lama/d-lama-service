using d_lama_service.Repositories.Core;
using Data;
using Data.ProjectEntities;

namespace d_lama_service.Repositories.ProjectRepositories
{
    /// <summary>
    /// DataPointSetRepository Class.
    /// </summary>
    public class DataPointSetRepository : Repository<DataPointSet>, IDataPointSetRepository
    {
        /// <summary>
        /// Constructor of DataPointSetRepository.
        /// </summary>
        /// <param name="context"> The DB context. </param>
        public DataPointSetRepository(DataContext context) : base(context) { }
    }
}
