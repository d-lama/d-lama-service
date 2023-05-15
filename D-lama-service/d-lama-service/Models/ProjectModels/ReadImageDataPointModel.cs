using Data.ProjectEntities;
using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.ProjectModels
{
    public class ReadImageDataPointModel
    {
        [Required]
        public int DataPointIndex { get; set; }

        [Required]
        public string FileName { get; set; }

        public ReadImageDataPointModel(ImageDataPoint imageDataPoint)
        {
            DataPointIndex = imageDataPoint.DataPointIndex;
            FileName = Path.GetFileName(imageDataPoint.Path);
        }
    }
}
