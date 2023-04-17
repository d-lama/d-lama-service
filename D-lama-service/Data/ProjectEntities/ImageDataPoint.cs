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

        public ImageDataPoint(string path, int dataSetId, int row)
        {
            Path = path;
            Row = row;
        }
    }
}
