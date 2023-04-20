using System.ComponentModel.DataAnnotations;

namespace Data.ProjectEntities
{
    /// <summary>
    /// Label set entity.
    /// </summary>
    public class LabelSet : Entity
    {
        [Required]
        public string LabelSetName { get; set; }
        public string? Description { get; set; }

        // navigation (EF core relationship mapping)
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public LabelSet(string labelSetName, string? description)
        {
            LabelSetName = labelSetName;
            Description = description;
        }

    }
}
