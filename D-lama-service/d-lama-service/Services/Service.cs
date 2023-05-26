using Data;

namespace d_lama_service.Services
{
    public class Service : IService
    {
        private DataContext _context;

        /// <summary>
        /// Constructor of UnitOfWork.
        /// </summary>
        /// <param name="context"> The DB context. </param>
        public Service(DataContext context)
        {
            _context = context;
        }

        protected async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Disposes the context.
        /// </summary>
        /// <returns> ValueTask. </returns>
        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}
