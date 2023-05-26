using d_lama_service.Models.ProjectModels;
using Data.ProjectEntities;
using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.DataPointModels
{
    public class ReadTextDataPointModel : ReadDataPointModel
    {
        [Required]
        public string Content { get; set; }

        public ReadTextDataPointModel(TextDataPoint textDataPoint, bool isLabelled) : base(textDataPoint.Id, isLabelled)
        {
            Content = textDataPoint.Content;
        }
    }
}
