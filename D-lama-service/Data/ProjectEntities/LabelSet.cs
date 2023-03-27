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

        public LabelSet(string labelSetName)
        {
            LabelSetName = labelSetName;
        }

    }
}
