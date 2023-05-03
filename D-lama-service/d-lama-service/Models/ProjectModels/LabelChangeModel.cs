using Data.ProjectEntities;
using System.ComponentModel.DataAnnotations;


namespace d_lama_service.Models.ProjectModels
{
    public class LabelChangeModel
    {
        [Required]
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public LabelChangeModel() { }
        public LabelChangeModel(Label label) 
        {
            Id = label.Id;
            Name = label.Name;
            Description = label.Description;
        }
    }
}
