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
        /// Saves the context.
        /// </summary>
        /// <returns> Task. </returns>
        Task SaveAsync();

    }
}
