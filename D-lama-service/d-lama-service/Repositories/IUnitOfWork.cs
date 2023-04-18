using d_lama_service.Repositories.ProjectRepositories;

namespace d_lama_service.Repositories
{
    /// <summary>
    /// Interface for the UnitOfWork.
    /// </summary>
    public interface IUnitOfWork : IAsyncDisposable
    {
        /// <summary>
        /// Gets the UserRepository.
        /// </summary>
        IUserRepository UserRepository { get; }

        /// <summary>
        /// Gets the ProjectRepository.
        /// </summary>
        IProjectRepository ProjectRepository { get; }

        /// <summary>
        /// Gets the TextDataPointRepository.
        /// </summary>
        ITextDataPointRepository TextDataPointRepository { get; }

        /// <summary>
        /// Gets the LabelRepository.
        /// </summary>
        ILabelRepository LabelRepository { get; }

        /// <summary>
        /// Saves the context.
        /// </summary>
        /// <returns> Task. </returns>
        Task SaveAsync();

    }
}
