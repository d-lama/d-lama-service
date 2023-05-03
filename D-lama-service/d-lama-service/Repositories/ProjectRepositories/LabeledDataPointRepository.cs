using d_lama_service.Repositories.Core;
using Data;
using Data.ProjectEntities;


namespace d_lama_service.Repositories.ProjectRepositories
{
    public class LabeledDataPointRepository : Repository<LabeledDataPoint>, ILabeledDataPointRepository
    {
        /// <summary>
        /// Constructor of LabeledDataPointRepository.
        /// </summary>
        /// <param name="context"> The DB context. </param>
        public LabeledDataPointRepository(DataContext context) : base(context) { }
    }
}
