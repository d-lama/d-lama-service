using d_lama_service.Repositories;
using d_lama_service.Repositories.ProjectRepositories;
using Data;

namespace d_lama_service.Repositories
{
    /// <summary>
    /// Unit of Work class to handle transactions as one unit.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private DataContext _context;
        private IExampleRepository? _exampleRepository;
        private IUserRepository? _userRepository;
        private IProjectRepository? _projectRepository;
        private IDataPointSetRepository? _dataPointSetRepository;
        private ILabelSetRepository? _labelSetRepository;
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

        /// <summary>
        /// UserRepository access with lazy loading.
        /// </summary>
        public IUserRepository UserRepository
        {
            // lazy loading
            get
            {

                if (_userRepository == null)
                {
                    _userRepository = new UserRepository(_context);
                }
                return _userRepository;
            }
        }

        /// <summary>
        /// ProjectRepository access with lazy loading.
        /// </summary>
        public IProjectRepository ProjectRepository
        {
            get
            {

                if (_projectRepository == null)
                {
                    _projectRepository = new ProjectRepository(_context);
                }
                return _projectRepository;
            }
        }

        /// <summary>
        /// DataPointSetRepository access with lazy loading.
        /// </summary>
        public IDataPointSetRepository DataPointSetRepository
        {
            get
            {
                if (_dataPointSetRepository == null)
                {
                    _dataPointSetRepository = new DataPointSetRepository(_context);
                }
                return _dataPointSetRepository;
            }
        }

        /// <summary>
        /// LabelSetRepository access with lazy loading.
        /// </summary>
        public ILabelSetRepository LabelSetRepository
        {
            get
            {
                if (_labelSetRepository == null)
                {
                    _labelSetRepository = new LabelSetRepository(_context);
                }
                return _labelSetRepository;
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
