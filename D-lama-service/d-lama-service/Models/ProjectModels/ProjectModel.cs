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
        public List<LabelChangeModel> LabelSets { get; set; }

        public ProjectModel() { }
        public ProjectModel(Project project)
        {
            ProjectName = project.Name;
            Description = project.Description;
            LabelSets = new List<LabelChangeModel>();
            foreach (var label in project.Labels) 
            {
                LabelSets.Add(new LabelChangeModel(label));
            } 
        }

    }
}
