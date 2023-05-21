using Data.ProjectEntities;
using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.ProjectModels
{
    public class ReadTextDataPointModel : TextDataPointModel
    {
        [Required]
        public int DataPointIndex { get; set; }

        [Required]
        public bool IsLabelled { get; set; }

        public ReadTextDataPointModel(TextDataPoint textDataPoint, bool isLabelled) : base(textDataPoint)
        {
            DataPointIndex = textDataPoint.DataPointIndex;
            IsLabelled = isLabelled;
        }
    }
}
