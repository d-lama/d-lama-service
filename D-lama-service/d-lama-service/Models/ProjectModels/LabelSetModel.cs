using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.ProjectModels
{
    public class LabelSetModel
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
