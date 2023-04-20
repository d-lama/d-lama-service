using Data.ProjectEntities;
using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.ProjectModels
{
    public class ProjectModel
    {
        [Required]
        public string ProjectName { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        public List<LabelSetChangeModel> LabelSets { get; set; }

        public ProjectModel() { }
        public ProjectModel(Project project)
        {
            ProjectName = project.ProjectName;
            Description = project.Description;
            LabelSets = new List<LabelSetChangeModel>();
            foreach (var labelSet in project.LabelSets) 
            {
                LabelSets.Add(new LabelSetChangeModel(labelSet));
            } 
        }

    }
}
