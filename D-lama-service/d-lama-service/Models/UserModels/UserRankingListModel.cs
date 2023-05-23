namespace d_lama_service.Models.UserModels
{
    public class UserRankingListModel
    {
        public int MyPosition { get; set; }
        public List<UserRankingModel> Ranking { get; set; }

        public UserRankingListModel(int myPosition, List<UserRankingModel> userRanking)
        {
            MyPosition = myPosition;
            Ranking = userRanking;
        }
    }
}
