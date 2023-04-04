using d_lama_service.Repositories.ProjectRepositories;

namespace d_lama_service.Repositories
{
    /// <summary>
    /// Interface for the UnitOfWork.
    /// </summary>
    public interface IUnitOfWork : IAsyncDisposable
    {
        /// <summary>
        /// Gets the ExampleRepository.
        /// </summary>
        IExampleRepository ExampleRepository{ get; }

        /// <summary>
        /// Gets the UserRepository.
        /// </summary>
        IUserRepository UserRepository { get; }

        /// <summary>
        /// Gets the ProjectRepository.
        /// </summary>
        IProjectRepository ProjectRepository { get; }

        /// <summary>
        /// Gets the DataPointSetRepository.
        /// </summary>
        IDataPointSetRepository DataPointSetRepository { get; }

        /// <summary>
        /// Gets the LabelSetRepository.
        /// </summary>
        ILabelSetRepository LabelSetRepository { get; }

        /// <summary>
        /// Saves the context.
        /// </summary>
        /// <returns> Task. </returns>
        Task SaveAsync();

    }
}
