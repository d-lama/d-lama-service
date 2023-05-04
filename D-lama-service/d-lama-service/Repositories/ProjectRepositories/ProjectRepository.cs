using d_lama_service.Repositories.Core;
using Data;
using Data.ProjectEntities;

namespace d_lama_service.Repositories.ProjectRepositories
{
    /// <summary>
    /// ProjectRepository Class.
    /// </summary>
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        /// <summary>
        /// Constructor of ProjectRepository.
        /// </summary>
        /// <param name="context"> The DB context. </param>
        public ProjectRepository(DataContext context) : base(context) { }
    }
}
