using Data;
using d_lama_service.Repositories.Core;

namespace d_lama_service.Repositories
{
    /// <summary>
    /// IProjectRepository Interface.
    /// </summary>
    public interface IProjectRepository : IRepository<Project>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Project> MyFancyQueryAsync(int id);
    }
}
