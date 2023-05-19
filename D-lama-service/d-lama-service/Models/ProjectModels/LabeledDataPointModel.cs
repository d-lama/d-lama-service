using Data.ProjectEntities;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace d_lama_service.Models.ProjectModels
{
    public class LabeledDataPointModel
    {
        public Dictionary<int, int> LabelIdToCountMap { get; set; }
        public int LabellersCount { get; set; } = 0;

        public LabeledDataPointModel(Dictionary<int,int> labelIdToCountMap) 
        {
            LabelIdToCountMap = labelIdToCountMap;
            foreach (var counts in labelIdToCountMap.Values) 
            {
                LabellersCount += counts;
            }
        }
    }
}
