using d_lama_service.Repositories.Core;
using Data;
using Microsoft.EntityFrameworkCore;

namespace d_lama_service.Repositories
{
    /// <summary>
    /// ExampleRepository Class.
    /// </summary>
    public class ExampleRepository : Repository<Example>, IExampleRepository
    {
        /// <summary>
        /// Constructor of ExampleRepository.
        /// </summary>
        /// <param name="context"> The DB context. </param>
        public ExampleRepository(DataContext context) : base(context){}

        public async Task<Example> MyFancyQueryAsync(int id)
        { 
            var entity = (await GetAllAsync()).First();
            entity.Description = "My fancy Descr.";
            return entity;
        }
    }
}
