using Data.ProjectEntities;
using System.ComponentModel.DataAnnotations;


namespace d_lama_service.Models.ProjectModels
{
    public class LabelSetChangeModel
    {
        [Required]
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public LabelSetChangeModel() { }
        public LabelSetChangeModel(LabelSet labelSet) 
        {
            Id = labelSet.Id;
            Name = labelSet.LabelSetName;
            Description = labelSet.Description;
        }
    }
}
