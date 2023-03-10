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
        /// Saves the context.
        /// </summary>
        /// <returns> Task. </returns>
        Task SaveAsync();

    }
}
