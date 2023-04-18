using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.ProjectModels
{
    public class EditProjectModel
    {
        public string? ProjectName { get; set; }
        
        public string? Description { get; set; }
        
        public LabelSetChangeModel[]? LabeSetChanges { get; set; }
    }
}
