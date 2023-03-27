using System.Linq.Expressions;
using Data;

namespace d_lama_service.Repositories.Core
{
    /// <summary>
    /// Generic repository interface.
    /// </summary>
    /// <typeparam name="TEntity"> The entity type. </typeparam>
    public interface IRepository<TEntity> where TEntity : Entity
    {
        /// <summary>
        /// Gets an entity by a provided id.
        /// </summary>
        /// <param name="id"> The id of the entity. </param>
        /// <returns> The entity. </returns>
        Task<TEntity?> GetAsync(int id);

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns> All entities. </returns>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Gets all entities that matches the predicated.
        /// </summary>
        /// <param name="predicate"> The predicate the entities need to match. </param>
        /// <returns> All entities which have matched the predicate. </returns>
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Adds an entity.
        /// </summary>
        /// <param name="entity"> The entity to add or update. </param>
        void Add(TEntity entity);

        /// <summary>
        /// Updates and entity.
        /// </summary>
        /// <param name="entity"> The entity to add or update. </param>
        void Update(TEntity entity);

        /// <summary>
        /// Adds or updates a range of entities.
        /// </summary>
        /// <param name="entities"> The entities to add or update. </param>
        void UpdateRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="entity">The entity to delete. </param>
        void Delete(TEntity entity);

        /// <summary>
        /// Deletes a range of entities. 
        /// </summary>
        /// <param name="entities"> The entities to delete. </param>
        void DeleteRange(IEnumerable<TEntity> entities);
    }
}
