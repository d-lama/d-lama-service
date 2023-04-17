using d_lama_service.Repositories.Core;
using Data;
using Data.ProjectEntities;

namespace d_lama_service.Repositories.ProjectRepositories
{
    /// <summary>
    /// LabelRepository Class.
    /// </summary>
    public class LabelRepository : Repository<Label>, ILabelRepository
    {
        /// <summary>
        /// Constructor of LabelRepository.
        /// </summary>
        /// <param name="context"> The DB context. </param>
        public LabelRepository(DataContext context) : base(context) { }
    }
}
