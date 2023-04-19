using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.ProjectEntities
{
    /// <summary>
    /// Project entity.
    /// </summary>W
    public class Project : Entity
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreationDate { get; set; }
        [Required]
        public DateTime UpdateDate { get; set; }
        [Required]
        public bool isReady { get; set; }

        // Required foreign key property
        public int OwnerId { get; set; }
        // Required reference navigation to principal
        public User Owner { get; set; } = null!;
        // Collections navigation containing dependents
        public ICollection<TextDataPoint> TextDataPoints { get; } = new List<TextDataPoint>();
        public ICollection<Label> Labels { get; } = new List<Label>();

        public Project(string name, string description)
        {
            Name = name;
            Description = description;
            OwnerId = ownerId;
            LabelSets = new List<LabelSet>();
            UpdateDate = DateTime.Now;
            isReady = false;
        }
    }
}
