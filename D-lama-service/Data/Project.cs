using System.ComponentModel.DataAnnotations;

namespace Data
{

    public class Project : Entity
    {
        [Required]
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int OwnerId { get; set; }
        public string? Description { get; set; }
        public int? DataSetId { get; set; }
        public int? LabelSetId { get; set; }
        public int? SettingsId { get; set; }
        public bool? isReady { get; set; }

        public Project(string name, int projId, int ownerId) 
        {
            Name = name;
            ProjectId = projId;
            OwnerId = ownerId;
        }
    }
}
