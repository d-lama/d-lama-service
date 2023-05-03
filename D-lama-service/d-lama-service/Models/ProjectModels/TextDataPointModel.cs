using Data.ProjectEntities;
using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.ProjectModels
{
    public class TextDataPointModel
    {
        [Required]
        public string Content { get; set; }

        public TextDataPointModel() { }

        public TextDataPointModel(TextDataPoint textDataPoint)
        {
            Content = textDataPoint.Content;
        }
    }
}
