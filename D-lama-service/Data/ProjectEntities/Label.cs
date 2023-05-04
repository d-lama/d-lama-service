using System.ComponentModel.DataAnnotations;

namespace Data.ProjectEntities
{
    /// <summary>
    /// Label entity.
    /// </summary>
    public class Label : Entity
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }

        // Required foreign key property
        public int ProjectId { get; set; }
        // Required reference navigation to principal
        public Project Project { get; set; } = null!;

        public ICollection<LabeledDataPoint> LabeledDataPoints { get; } = new List<LabeledDataPoint>();

        public Label(string name, string? description)
        {
            Name = name;
            Description = description;
        }

    }
}
