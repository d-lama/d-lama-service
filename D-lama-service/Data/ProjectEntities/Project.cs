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
        public string ProjectName { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int OwnerId { get; set; }
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedDate { get; set; }

        // navigation (EF core relationship mapping)
        public ICollection<DataPointSet> DataPointSets { get; }
        public ICollection<LabelSet> LabelSets { get; }

        public Project(string projectName, string description, int ownerId)
        {
            ProjectName = projectName;
            Description = description;
            OwnerId = ownerId;
            DataPointSets = new List<DataPointSet>();
            LabelSets = new List<LabelSet>();
        }
    }
}
