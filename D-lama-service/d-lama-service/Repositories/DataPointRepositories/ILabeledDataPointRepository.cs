using Data.ProjectEntities;
using d_lama_service.Repositories.Core;

namespace d_lama_service.Repositories.DataPointRepositories
{
    public interface ILabeledDataPointRepository : IRepository<LabeledDataPoint>
    {
        /// <summary>
        /// Gets Labelling statistics about a datapoint. 
        /// </summary>
        /// <param name="dataPointId"> The id of the datapoint. </param>
        /// <returns> A dictionary containing the mapping of a LabelId and the number of counts (how much a labeller has assigned this label to the datapoint). </returns>
        Task<Dictionary<int, int>> GetLabeledDataPointStatisticAsync(int dataPointId);

        /// <summary>
        /// Gets a mapping between user id and the percentage of the labeled data points in a list of data point ids. 
        /// </summary>
        /// <param name="dataPointIds"> The list of data point ids.</param>
        /// <returns> A dictionary containg the mapping of a UserId and the percentage of the labeled data compared to the whole list of provided datapoints (1 >= x >= 0).</returns>
        Task<Dictionary<int, float>> GetLabeledDataPointsProcessOfUser(IEnumerable<int> dataPointIds);
    }
}
