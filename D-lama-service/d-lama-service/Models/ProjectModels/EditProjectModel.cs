using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.ProjectModels
{
    public class EditProjectModel
    {
        public string? Name { get; set; }
        
        public string? Description { get; set; }
        
        public LabelChangeModel[]? LabeSetChanges { get; set; }
    }
}
