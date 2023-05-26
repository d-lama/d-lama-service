using d_lama_service.Repositories.Core;
using Data;
using Data.ProjectEntities;

namespace d_lama_service.Repositories.DataPointRepositories
{
    /// <summary>
    /// TextDataPointRepository Class.
    /// </summary>
    public class TextDataPointRepository : Repository<TextDataPoint>, ITextDataPointRepository
    {
        /// <summary>
        /// Constructor of TextDataPointRepository.
        /// </summary>
        /// <param name="context"> The DB context. </param>
        public TextDataPointRepository(DataContext context) : base(context) { }
    }
}
