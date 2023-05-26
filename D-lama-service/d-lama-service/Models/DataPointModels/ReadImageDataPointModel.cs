using Data.ProjectEntities;
using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.DataPointModels
{
    public class ReadImageDataPointModel : ReadDataPointModel
    {
        [Required]
        public string FileName { get; set; }

        public ReadImageDataPointModel(ImageDataPoint imageDataPoint, bool isLabelled) : base(imageDataPoint.DataPointIndex, isLabelled)
        {
            FileName = Path.GetFileName(imageDataPoint.Path);
        }
    }
}
