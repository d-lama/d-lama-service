using Data.ProjectEntities;
using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.ProjectModels
{
    public class DetailedProjectModel : ProjectModel
    {
        [Required]
        public int Id { get; set; }

        public DetailedProjectModel(Project project) : base(project)
        {
            Id = project.Id;
        }
    }
}
