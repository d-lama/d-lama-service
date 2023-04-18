﻿using d_lama_service.Repositories;
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
        private IUserRepository? _userRepository;
        private IProjectRepository? _projectRepository;
        private ITextDataPointRepository? _textDataPointRepository;
        private ILabelRepository? _labelRepository;
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
        /// TextDataPointRepository access with lazy loading.
        /// </summary>
        public ITextDataPointRepository TextDataPointRepository
        {
            get
            {
                if (_textDataPointRepository == null)
                {
                    _textDataPointRepository = new TextDataPointRepository(_context);
                }
                return _textDataPointRepository;
            }
        }

        /// <summary>
        /// LabelRepository access with lazy loading.
        /// </summary>
        public ILabelRepository LabelRepository
        {
            get
            {
                if (_labelRepository == null)
                {
                    _labelRepository = new LabelRepository(_context);
                }
                return _labelRepository;
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