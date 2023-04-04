using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.ProjectModels
{
    public class ProjectModel
    {
        [Required]
        public string ProjectName { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
