using Data;
using d_lama_service.Repositories.Core;

namespace d_lama_service.Repositories
{
    /// <summary>
    /// IExampleRepository Interface.
    /// </summary>
    public interface IExampleRepository : IRepository<Example>
    {
        /// <summary>
        /// This is a query only needed by for the ExampleRepository... => makes no sense to put it into IRepository.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Example> MyFancyQueryAsync(int id);
    }
}
