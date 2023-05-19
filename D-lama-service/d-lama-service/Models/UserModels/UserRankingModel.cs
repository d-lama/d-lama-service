namespace d_lama_service.Models.UserModels
{
    public class UserRankingModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float Percentage { get; set; }

        public UserRankingModel(int id, string name, float percentage) 
        {
            Id = id;
            Name = name;
            Percentage = percentage;
        }   

    }
}
