using d_lama_service.Repositories.Core;
using Data;
using Microsoft.EntityFrameworkCore;

namespace d_lama_service.Repositories
{
    /// <summary>
    /// Unit of Work class to handle transactions as one unit.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private DataContext _context;
        private IExampleRepository? _exampleRepository;
        private IProjectRepository? _projectRepository;
        // all repositories here ...

        /// <summary>
        /// Constructor of UnitOfWork.
        /// </summary>
        /// <param name="context"> The DB context. </param>
        public UnitOfWork(DataContext context)
        {
            _context = context; 
        }

        /// <summary>
        /// ExampleRepository access with lazy loading.
        /// </summary>
        public IExampleRepository ExampleRepository
        {
            // lazy loading
            get
            {

                if (_exampleRepository == null)
                {
                    _exampleRepository = new ExampleRepository(_context);
                }
                return _exampleRepository;
            }
        }

        public IProjectRepository ProjectRepository
        {
            // lazy loading
            get
            {

                if (_exampleRepository == null)
                {
                    _projectRepository = new ProjectRepository(_context);
                }
                return _projectRepository;
            }
        }

        public async Task SaveAsync()
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
