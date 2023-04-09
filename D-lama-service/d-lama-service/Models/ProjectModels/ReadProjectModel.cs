using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.ProjectModels
{
    public class ReadProjectModel : ProjectModel
    {
        [Required]
        public string OwnerName { get; set; }

        [Required]
        public string DataPointSetName { get; set; }

        [Required]
        public string LabelSetName { get; set; }
    }
}
