using d_lama_service.Models.ProjectModels;
using d_lama_service.Models.UserModels;

namespace d_lama_service.Models.DataPointModels
{
    public class LabeledDataModel
    {
        public ProjectModel Project { get; set; }
        public List<LabellerModel> Labellers { get; set; }
        public Dictionary<int, LabeledDataPointModel> DataPoints { get; set; }
    }
}
