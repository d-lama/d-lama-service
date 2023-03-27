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
        public int DataSetId { get; set; }
        [Required]
        public int LabelSetId { get; set; }
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedDate { get; set; }

        public Project(string projectName, string description, int ownerId, int dataSetId, int labelSetId)
        {
            ProjectName = projectName;
            Description = description;
            OwnerId = ownerId;
            DataSetId = dataSetId;
            LabelSetId = labelSetId;
        }
    }
}
