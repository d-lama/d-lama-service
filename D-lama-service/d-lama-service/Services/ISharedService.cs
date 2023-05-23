using Data;
using Data.ProjectEntities;
using System.Linq.Expressions;

namespace d_lama_service.Services
{
    public interface ISharedService : IService
    {
        Task<User> GetAuthenticatedUserAsync(HttpContext httpContext);
        Task<Project> GetProjectWithOwnerCheckAsync(int projectId, HttpContext httpContext, params Expression<Func<Project, object>>[] includes);
    }
}