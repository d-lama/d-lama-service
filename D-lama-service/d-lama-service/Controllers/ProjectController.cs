using Microsoft.AspNetCore.Mvc;
using Data.ProjectEntities;
using d_lama_service.Models.ProjectModels;
using Data;
using Microsoft.AspNetCore.Authorization;
using d_lama_service.Middleware;
using d_lama_service.Models.UserModels;
using d_lama_service.Services;

namespace d_lama_service.Controllers
{
    /// <summary>
    /// Project controller handles the requests related to a project.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ISharedService _sharedService;

        /// <summary>
        /// Constructor of the ProjectController.
        /// </summary>
        /// <param name="unitOfWork"> The unitOfWork for handling db access. </param>
        /// <param name="environment"> The web hosting environment. </param>
        public ProjectController(IProjectService projectService, ISharedService sharedService) 
        {
            _projectService = projectService;
            _sharedService = sharedService;
        }

        /// <summary>
        /// Retrieves a list of all projects.
        /// </summary>
        /// <returns> A list of projects or Null if there are no projects at all. </returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
            List<DetailedProjectModel> projectList = await _projectService.GetAllProjectsAsync(user);
            return Ok(projectList);
        }

        /// <summary>
        /// Retrieves a list of projects by a given admin user.
        /// </summary>
        /// <returns> A list of the projects created by logged in admin user. </returns>
        [AdminAuthorize]
        [HttpGet]
        [Route("My")]
        public async Task<IActionResult> GetMyProjects()
        {
            User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
            return Ok(await _projectService.GetProjectsOfOwnerAsync(user.Id));
        }

        /// <summary>
        /// Retrieves a project with a given project ID.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
            DetailedProjectModel project = await _projectService.GetProjectAsync(id, user);
            return Ok(project);
        }

        /// <summary>
        /// Creates a new project with the data passed in the request body.
        /// </summary>
        /// <param name="projectForm"> The project form containing all needed information to create a project. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [AdminAuthorize]
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProjectModel projectForm)
        {
            User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
            int projectId = await _projectService.CreateProjectAsync(user, projectForm);
            var createdResource = new { id = projectId };
            return CreatedAtAction(nameof(Get), createdResource, createdResource);
        }

        /// <summary>
        /// Edits a project with the data passed in the request body.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <param name="projectForm"> The project form containing all needed information to edit a project. </param>
        /// <returns> The updated project. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromBody] EditProjectModel projectForm)
        {
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(id, HttpContext, e => e.Labels);
            await _projectService.UpdateProjectAsync(project, projectForm);
            return await Get(id);
        }


        /// <summary>
        /// Deletes a project with a given project ID.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(id, HttpContext);
            await _projectService.DeleteProjectAsync(project);            
            return Ok();
        }

        /// <summary>
        /// Adds labels to an existing project.
        /// </summary>
        /// <param name="id"> The id of the project. </param>
        /// <param name="newLabels"> The new labels. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpPost("{id:int}/Labels/")]
        public async Task<IActionResult> AddLabels(int id, [FromBody] LabelSetModel[] newLabels) 
        {
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(id, HttpContext, e => e.Labels);
            await _projectService.CreateLablesAsync(project, newLabels);
            return Ok();
        }

        /// <summary>
        /// Removes a label from the project. Only works if the label was not used.
        /// </summary>
        /// <param name="id"> The id of the project. </param>
        /// <param name="labelId"> The id of the label. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpDelete("{id:int}/Labels/{labelId:int}")]
        public async Task<IActionResult> RemoveLabel(int id, int labelId)
        {
            await _sharedService.GetProjectWithOwnerCheckAsync(id, HttpContext);
            await _projectService.DeleteLabelAsync(id, labelId);
            return Ok();
        }

        /// <summary>
        /// Gets a ranking overview for a project. Get a ranking table with the processes of each user.
        /// </summary>
        /// <param name="id"> The id of the project. </param>
        /// <returns> The ranking on success, else an 400 error. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpGet("{id:int}/Ranking")]
        public async Task<IActionResult> GetRanking(int id) 
        {
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(id, HttpContext, e => e.DataPoints);
            List<UserRankingModel> rankingList = await _projectService.GetProjectRankingList(project);
            return Ok(rankingList);
        }
    }
}
