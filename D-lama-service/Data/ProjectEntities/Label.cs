using System.ComponentModel.DataAnnotations;

namespace Data.ProjectEntities
{
    /// <summary>
    /// Label entity.
    /// </summary>
    public class Label : Entity
    {
        [Required]
        public string Content { get; set; }

        // Required foreign key property
        public int ProjectId { get; set; }
        // Required reference navigation to principal
        public Project Project { get; set; } = null!;
        // Collections navigation containing dependents
        public ICollection<TextDataPoint> TextDataPoints { get; } = new List<TextDataPoint>();

        public Label(string content)
        {
            Content = content;
        }

    }
}
