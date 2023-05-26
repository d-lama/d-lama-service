using d_lama_service.Repositories.Core;
using Data;

namespace d_lama_service.Repositories.UserRepositories
{
    /// <summary>
    /// UserRepository abstracts the data accessing. 
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        /// <summary>
        /// Constructor of the UserRepository.
        /// </summary>
        /// <param name="context"> The database context. </param>
        public UserRepository(DataContext context) : base(context) { }
    }
}
