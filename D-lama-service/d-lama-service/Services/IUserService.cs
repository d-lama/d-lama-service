using d_lama_service.Models.UserModels;
using Data;

namespace d_lama_service.Services
{
    /// <summary>
    /// Interface of the UserService.
    /// </summary>
    public interface IUserService : IService
    {
        /// <summary>
        /// Creates a user from a registering request. 
        /// </summary>
        /// <param name="registerRequest"> The user request. </param>
        /// <returns> The id of the new created user. </returns>
        Task<int> CreateUserAsync(RegisterModel registerRequest);
        
        /// <summary>
        /// Gets a user.
        /// </summary>
        /// <param name="id"> The id of the wanted user. </param>
        /// <returns> The user. </returns>
        Task<User> GetUserAsync(int id);

        /// <summary>
        /// Updates the user. 
        /// </summary>
        /// <param name="user"> The user to update. </param>
        /// <param name="modifiedUser"> The new user (the update). </param>
        /// <returns></returns>
        Task UpdateUserAsync(User user, EditUserModel modifiedUser);

        /// <summary>
        /// Gets the authentication token for a login request. 
        /// </summary>
        /// <param name="loginRequest"> The login request. </param>
        /// <returns> The authentication token. </returns>
        Task<string> GetAuthTokenAsync(LoginModel loginRequest);

        /// <summary>
        /// Gets the user ranking table.
        /// </summary>
        /// <param name="authUser"> The authenticated user. </param>
        /// <returns> The user ranking table. </returns>
        Task<UserRankingListModel> GetUserRankingTableAsync(User authUser);
    }
}
