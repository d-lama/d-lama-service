using System.ComponentModel.DataAnnotations;

namespace Data
{

    public class Project : Entity
    {
        [Required]
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int OwnerId { get; set; }

        public Project(string name, int projectId, int ownerId) 
        {
            Name = name;
            ProjectId = projectId;
            OwnerId = ownerId;
        }

        public string? Description { get; set; }
        public int? DataSetId { get; set; }
        public int? LabelSetId { get; set; }
        public int? SettingsId { get; set; }
        public bool? isReady { get; set; }
    }
}
