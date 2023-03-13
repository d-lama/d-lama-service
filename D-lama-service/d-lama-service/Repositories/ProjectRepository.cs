using d_lama_service.Repositories.Core;
using Data;
using Microsoft.EntityFrameworkCore;

namespace d_lama_service.Repositories
{
    /// <summary>
    /// ExampleRepository Class.
    /// </summary>
    public class ProjectRepository : Repository<Project>
    {
        /// <summary>
        /// Constructor of ExampleRepository.
        /// </summary>
        /// <param name="context"> The DB context. </param>
        public ProjectRepository(DataContext context) : base(context){}

        public async Task<Project> MyFancyQueryAsync(int id)
        { 
            var entity = (await GetAllAsync()).First();
            entity.Description = "My fancy Descr.";
            return entity;
        }
    }
}
