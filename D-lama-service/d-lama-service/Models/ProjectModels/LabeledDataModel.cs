using Data.ProjectEntities;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace d_lama_service.Models.ProjectModels
{
    public class LabeledDataModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public Dictionary<int, Dictionary<int, int>> LabeledData { get; set; }

        public LabeledDataModel(Project project, IEnumerable<LabeledDataPoint>? labeledDataPoints) 
        {
            ProjectId = project.Id;
            ProjectName = project.Name;
            LabeledData = new Dictionary<int, Dictionary<int, int>>();
            MapLabeledDataPoints(project, labeledDataPoints);
        }

        private void MapLabeledDataPoints(Project project, IEnumerable<LabeledDataPoint>? labeledDataPoints) 
        {
            if (labeledDataPoints != null && labeledDataPoints.Any())
            {
                var groupedDataPoints = labeledDataPoints!
                    .GroupBy(e => new { DataPointId = e.DataPointId, LabelId = e.LabelId })
                    .Select(e => new { DataPointId = e.Key.DataPointId, LabelId = e.Key.LabelId, Count = e.Count() });

                foreach (var groupedPoint in groupedDataPoints)
                {
                    var dataPointId = groupedPoint.DataPointId;
                    if (LabeledData.ContainsKey(dataPointId))
                    {
                        LabeledData[dataPointId].Add(groupedPoint.LabelId, groupedPoint.Count);
                    }
                    else
                    {
                        var labelCountMap = new Dictionary<int, int>() { { groupedPoint.LabelId, groupedPoint.Count } };
                        LabeledData.Add(dataPointId, labelCountMap);
                    }
                }
            }
        }
    }
}
