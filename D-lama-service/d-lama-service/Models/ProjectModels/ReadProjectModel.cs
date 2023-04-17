using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.ProjectModels
{
    public class ReadProjectModel : ProjectModel
    {
        [Required]
        public string OwnerName { get; set; }
    }
}
