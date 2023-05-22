using d_lama_service.Repositories.Core;
using Data;
using Data.ProjectEntities;
using Microsoft.EntityFrameworkCore;

namespace d_lama_service.Repositories.ProjectRepositories
{
    public class LabeledDataPointRepository : Repository<LabeledDataPoint>, ILabeledDataPointRepository
    {
        /// <summary>
        /// Constructor of LabeledDataPointRepository.
        /// </summary>
        /// <param name="context"> The DB context. </param>
        public LabeledDataPointRepository(DataContext context) : base(context) { }

        public async Task<Dictionary<int, int>> GetLabeledDataPointStatisticAsync(int dataPointId)
        {
            var result = new Dictionary<int, int>();
            await Entities
                .Where(e => e.DataPointId == dataPointId)
                .GroupBy(e => e.LabelId)
                .Select(e => new { e.Key, Count = e.Count() })
                .ForEachAsync(e => { result.Add(e.Key, e.Count); });
            return result;
        }

        public async Task<Dictionary<int, float>> GetLabeledDataPointsProcessOfUser(IEnumerable<int> dataPointIds) 
        {
            var result = new Dictionary<int, float>();
            await Entities
                .Where(e => dataPointIds.Contains(e.DataPointId))
                .GroupBy(e => e.UserId)
                .Select(e => new { e.Key, Percentage = (float)e.Count() / (float)dataPointIds.Count() })
                .ForEachAsync(e => { result.Add(e.Key, e.Percentage); });
            return result;
        }
    }
}
