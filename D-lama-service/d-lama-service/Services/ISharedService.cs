using Data;
using Data.ProjectEntities;
using System.Linq.Expressions;

namespace d_lama_service.Services
{
    /// <summary>
    /// Interface of the SharedService.
    /// </summary>
    public interface ISharedService : IService
    {
        /// <summary>
        /// Gets the authenticated user from the request context. 
        /// </summary>
        /// <param name="httpContext"> The request context. </param>
        /// <returns></returns>
        Task<User> GetAuthenticatedUserAsync(HttpContext httpContext);

        /// <summary>
        /// Gets the project with checking for the owner.
        /// If someone else is the owner than the person making the request, an exception is thrown. 
        /// </summary>
        /// <param name="projectId"> The id of the project. </param>
        /// <param name="httpContext"> The request context. </param>
        /// <param name="includes"> The includes, which should be included in the project (tables to join). </param>
        /// <returns> The project with its included properties. </returns>
        Task<Project> GetProjectWithOwnerCheckAsync(int projectId, HttpContext httpContext, params Expression<Func<Project, object>>[] includes);
    }
}