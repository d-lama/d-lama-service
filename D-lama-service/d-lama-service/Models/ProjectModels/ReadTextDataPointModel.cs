using Data.ProjectEntities;
using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.ProjectModels
{
    public class ReadTextDataPointModel : TextDataPointModel
    {
        [Required]
        public int DataPointIndex { get; set; }

        public ReadTextDataPointModel(TextDataPoint textDataPoint) : base(textDataPoint)
        {
            DataPointIndex = textDataPoint.DataPointIndex;
        }
    }
}
