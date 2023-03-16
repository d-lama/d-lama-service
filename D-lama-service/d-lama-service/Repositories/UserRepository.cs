using d_lama_service.Repositories.Core;
using Data;

namespace d_lama_service.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {

        public UserRepository(DataContext context) : base(context) { }
    }
}
