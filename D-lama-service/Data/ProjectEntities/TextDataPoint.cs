using System.ComponentModel.DataAnnotations;

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
        public Project Project { get; set; } = null!;
        // Optional reference navigation to principal
        public User? Labeler { get; set; }
        public Label? Label { get; set; }

        public TextDataPoint(string content, int dataPointIndex)
        {
            Content = content;
            DataPointIndex = dataPointIndex;
            UpdateDate = DateTime.Now;
            Version = 1;
        }
    }
}
