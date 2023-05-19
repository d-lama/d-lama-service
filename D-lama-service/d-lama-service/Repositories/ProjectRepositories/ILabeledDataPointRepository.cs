using Data.ProjectEntities;
using d_lama_service.Repositories.Core;

namespace d_lama_service.Repositories.ProjectRepositories
{
    public interface ILabeledDataPointRepository : IRepository<LabeledDataPoint>
    {
        /// <summary>
        /// Gets Labelling statistics about a datapoint. 
        /// </summary>
        /// <param name="dataPointId"> The id of the datapoint. </param>
        /// <returns> A dictionary containing the mapping of a LabelId and the number of counts (how much a labeller has assigned this label to the datapoint). </returns>
        Task<Dictionary<int, int>> GetLabeledDataPointStatisticAsync(int dataPointId);
    }
}
