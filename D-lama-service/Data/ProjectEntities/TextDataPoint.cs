using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Data.ProjectEntities
{
    /// <summary>
    /// Text data point entity.
    /// </summary>
    public class TextDataPoint : DataPoint
    {
        [Required]
        public string Content { get; set; }

        // Required foreign key property
        public int ProjectId { get; set; }
        // Required reference navigation to principal
        [JsonIgnore]
        public Project Project { get; set; } = null!;

        public TextDataPoint(string content, int dataPointIndex)
        {
            Content = content;
            DataPointIndex = dataPointIndex;
            UpdateDate = DateTime.UtcNow;
            Version = 1;
        }
    }
}
