using Data.ProjectEntities;
using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.DataPointModels
{
    public abstract class ReadDataPointModel
    {
        [Required]
        public int DataPointIndex { get; set; }

        [Required]
        public bool IsLabelled { get; set; }

        public ReadDataPointModel(int dataPointId, bool isLabelled)
        {
            DataPointIndex = dataPointId;
            IsLabelled = isLabelled;
        }
    }
}
