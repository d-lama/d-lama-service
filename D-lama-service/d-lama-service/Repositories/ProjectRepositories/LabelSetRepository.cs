using d_lama_service.Repositories.Core;
using Data;
using Data.ProjectEntities;

namespace d_lama_service.Repositories.ProjectRepositories
{
    /// <summary>
    /// LabelSetRepository Class.
    /// </summary>
    public class LabelSetRepository : Repository<LabelSet>, ILabelSetRepository
    {
        /// <summary>
        /// Constructor of LabelSetRepository.
        /// </summary>
        /// <param name="context"> The DB context. </param>
        public LabelSetRepository(DataContext context) : base(context) { }
    }
}
