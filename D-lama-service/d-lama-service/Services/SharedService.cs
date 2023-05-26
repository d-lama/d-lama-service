using d_lama_service.Models;
using d_lama_service.Repositories.ProjectRepositories;
using d_lama_service.Repositories.UserRepositories;
using Data;
using Data.ProjectEntities;
using System.Linq.Expressions;
using System.Net;

namespace d_lama_service.Services
{
    /// <summary>
    /// Implementation of the ISharedService interface.
    /// The shared service handles the domain specific logic which are shared by different controllers.
    /// </summary>
    public class SharedService : Service, ISharedService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;

        /// <summary>
        /// Constructor of the SharedService.
        /// </summary>
        /// <param name="context"> The database context. </param>
        public SharedService(DataContext context) : base(context)
        {
            _userRepository = new UserRepository(context);
            _projectRepository = new ProjectRepository(context);
        }

        /// <summary>
        /// Gets the authenticated user.
        /// </summary>
        /// <param name="httpContext"> The http context, where the user is authenticated. </param>
        /// <returns></returns>
        public async Task<User> GetAuthenticatedUserAsync(HttpContext httpContext)
        {
            var userId = int.Parse(httpContext.User.FindFirst(Tokenizer.UserIdClaim)?.Value!); // on error throw
            return (await _userRepository.GetAsync(userId))!;
        }


        /// <summary>
        /// Gets a project with checking if the user is owner of the project. 
        /// </summary>
        /// <param name="projectId"> The id of the project. </param>
        /// <param name="includes"> The other tables to load (join). </param>
        /// <returns> The found project with its dependencies. </returns>
        /// <exception cref="RESTException"> Throws Rest Excetption if project is not found or the current user is not the owner. </exception>
        public async Task<Project> GetProjectWithOwnerCheckAsync(int projectId, HttpContext httpContext, params Expression<Func<Project, object>>[] includes)
        {
            Project? project;
            if (includes.Any())
            {
                project = await _projectRepository.GetDetailsAsync(projectId, includes);
            }
            else
            {
                project = await _projectRepository.GetAsync(projectId);
            }

            if (project == null)
            {
                throw new RESTException(HttpStatusCode.NotFound, $"Project with id {projectId} does not exist.");
            }

            var user = await GetAuthenticatedUserAsync(httpContext);
            if (project.Owner != user)
            {
                throw new RESTException(HttpStatusCode.Unauthorized, $"Only the owner of the project can modify it.");
            }

            return project;
        }
    }
}
