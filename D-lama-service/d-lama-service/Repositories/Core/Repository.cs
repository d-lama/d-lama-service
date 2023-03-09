using Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace d_lama_service.Repositories.Core
{
    /// <summary>
    /// The generic repository. 
    /// </summary>
    /// <typeparam name="TEntity"> The entity type. </typeparam>
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
    {
        protected readonly DbContext Context;
        protected DbSet<TEntity> Entities;

        /// <summary>
        /// Constructor of the Repository.
        /// </summary>
        /// <param name="context"> The whole DB context. </param>
        public Repository(DbContext context) 
        {
            Context = context;
            Entities = Context.Set<TEntity>();
        }
        
        public async Task<TEntity?> GetAsync(int id) 
        {
            return await Entities.FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await Entities.ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Entities.Where(predicate).ToListAsync();
        }

        public void Update(TEntity entity)
        {
            Entities.Update(entity); 
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            Entities.UpdateRange(entities);
        }
        public void Delete(TEntity entity)
        {
            Entities.Remove(entity);
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            Entities.RemoveRange(entities);
        }
    }
}
