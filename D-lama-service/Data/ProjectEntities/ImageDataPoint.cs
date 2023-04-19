using System.ComponentModel.DataAnnotations;

namespace Data.ProjectEntities
{
    /// <summary>
    /// Image data point entity.
    /// </summary>
    public class ImageDataPoint : DataPoint
    {
        [Required]
        public string Path { get; set; }

        public ImageDataPoint(string path, int dataPointIndex)
        {
            Path = path;
            DataPointIndex = dataPointIndex;
            UpdateDate = DateTime.Now;
            Version = 1;
        }
    }
}
