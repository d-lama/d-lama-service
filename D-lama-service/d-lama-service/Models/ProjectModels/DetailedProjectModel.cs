using Data.ProjectEntities;
using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.ProjectModels
{
    public class DetailedProjectModel : ProjectModel
    {
        [Required]
        public int Id { get; set; }

        public int DataPointsCount { get; set; }
        public int LabeledDataPointsCount { get; set; }

        public DetailedProjectModel(Project project, int dataPointsCount, int labeledDataPointsCount) : base(project)
        {
            Id = project.Id;
            DataPointsCount = dataPointsCount;
            LabeledDataPointsCount = labeledDataPointsCount;
        }
    }
}
