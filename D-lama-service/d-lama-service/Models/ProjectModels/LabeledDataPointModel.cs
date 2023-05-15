using Data.ProjectEntities;

namespace d_lama_service.Models.ProjectModels
{
    public class LabeledDataPointModel
    {
        public int Counters { get; set; }
        public Dictionary<int,int> LabelsToCount { get; set; } = new Dictionary<int,int>();

        public LabeledDataPointModel(IEnumerable<LabeledDataPoint> labeledDataPoints) 
        {
            foreach (var datapoints in labeledDataPoints) 
            {
                if()
            }
        }

    }
}
