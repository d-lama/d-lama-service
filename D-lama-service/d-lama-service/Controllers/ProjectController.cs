using Microsoft.AspNetCore.Mvc;
using Data.ProjectEntities;
using d_lama_service.Models.ProjectModels;
using Data;
using Microsoft.AspNetCore.Authorization;
using d_lama_service.Middleware;
using d_lama_service.Models.UserModels;
using d_lama_service.Services;
using Microsoft.EntityFrameworkCore;

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
        private readonly ILoggerService _loggerService;

        /// <summary>
        /// Constructor of the ProjectController.
        /// </summary>
        /// <param name="unitOfWork"> The unitOfWork for handling db access. </param>
        /// <param name="environment"> The web hosting environment. </param>
        public ProjectController(IProjectService projectService, ISharedService sharedService, ILoggerService loggerService)
        {
            _projectService = projectService;
            _sharedService = sharedService;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Retrieves a list of all projects.
        /// </summary>
        /// <returns> A list of projects or Null if there are no projects at all. </returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                List<DetailedProjectModel> projectList = await _projectService.GetAllProjectsAsync(user);
                _loggerService.LogInformation(user.Id, "All projects were retrieved successfully.");
                return Ok(projectList);
            }
            catch (ArgumentNullException ex)
            {
                _loggerService.LogException(ex);
                return BadRequest("One or more required parameters are null.");
            }
            catch (InvalidOperationException ex)
            {
                _loggerService.LogException(ex);
                return Conflict("Operation not allowed due to current object state.");
            }
            catch (Exception ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An unexpected error occurred while retrieving the data points.");
            }
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
            try
            {
                User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                _loggerService.LogInformation(user.Id, $"All projects of the user were retrieved.");
                return Ok(await _projectService.GetProjectsOfOwnerAsync(user.Id));
            }
            catch (ArgumentNullException ex)
            {
                _loggerService.LogException(ex);
                return BadRequest("One or more required parameters are null.");
            }
            catch (InvalidOperationException ex)
            {
                _loggerService.LogException(ex);
                return Conflict("Operation not allowed due to current object state.");
            }
            catch (Exception ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An unexpected error occurred while retrieving the data points.");
            }
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
            try
            {
                var user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                DetailedProjectModel project = await _projectService.GetProjectAsync(id, user);
                _loggerService.LogInformation(user.Id, $"The project with id {project.Id} was retrieved.");
                return Ok(project);
            }
            catch (ArgumentNullException ex)
            {
                _loggerService.LogException(ex);
                return BadRequest("One or more required parameters are null.");
            }
            catch (InvalidOperationException ex)
            {
                _loggerService.LogException(ex);
                return Conflict("Operation not allowed due to current object state.");
            }
            catch (Exception ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An unexpected error occurred while retrieving the data points.");
            }
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
            try
            {
                User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                int projectId = await _projectService.CreateProjectAsync(user, projectForm);
                var createdResource = new { id = projectId };
                _loggerService.LogInformation(user.Id, $"A new project with id {projectId} has been created.");
                return CreatedAtAction(nameof(Get), createdResource, createdResource);
            }
            catch (DbUpdateException ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An error occurred while updating the database.");
            }
            catch (ArgumentNullException ex)
            {
                _loggerService.LogException(ex);
                return BadRequest("One or more required parameters are null.");
            }
            catch (InvalidOperationException ex)
            {
                _loggerService.LogException(ex);
                return Conflict("Operation not allowed due to current object state.");
            }
            catch (Exception ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An unexpected error occurred while retrieving the data points.");
            }
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
            try
            {
                User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(id, HttpContext, e => e.Labels);
                await _projectService.UpdateProjectAsync(project, projectForm);
                _loggerService.LogInformation(user.Id, $"Project {project.Id} was edited.");
                return await Get(id);
            }
            catch (DbUpdateException ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An error occurred while updating the database.");
            }
            catch (ArgumentNullException ex)
            {
                _loggerService.LogException(ex);
                return BadRequest("One or more required parameters are null.");
            }
            catch (InvalidOperationException ex)
            {
                _loggerService.LogException(ex);
                return Conflict("Operation not allowed due to current object state.");
            }
            catch (Exception ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An unexpected error occurred while retrieving the data points.");
            }
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
            try
            {
                User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(id, HttpContext);
                _loggerService.LogInformation(user.Id, $"Project {project.Id} was deleted.");
                await _projectService.DeleteProjectAsync(project);
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An error occurred while updating the database.");
            }
            catch (ArgumentNullException ex)
            {
                _loggerService.LogException(ex);
                return BadRequest("One or more required parameters are null.");
            }
            catch (InvalidOperationException ex)
            {
                _loggerService.LogException(ex);
                return Conflict("Operation not allowed due to current object state.");
            }
            catch (Exception ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An unexpected error occurred while retrieving the data points.");
            }
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
            try
            {
                User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(id, HttpContext, e => e.Labels);
                await _projectService.CreateLablesAsync(project, newLabels);
                _loggerService.LogInformation(user.Id, $"Labels for project {project.Id} were created.");
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An error occurred while updating the database.");
            }
            catch (ArgumentNullException ex)
            {
                _loggerService.LogException(ex);
                return BadRequest("One or more required parameters are null.");
            }
            catch (InvalidOperationException ex)
            {
                _loggerService.LogException(ex);
                return Conflict("Operation not allowed due to current object state.");
            }
            catch (Exception ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An unexpected error occurred while retrieving the data points.");
            }
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
            try
            {
                User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                await _sharedService.GetProjectWithOwnerCheckAsync(id, HttpContext);
                await _projectService.DeleteLabelAsync(id, labelId);
                _loggerService.LogInformation(user.Id, $"Label {labelId} of project {id} was deleted.");
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An error occurred while updating the database.");
            }
            catch (ArgumentNullException ex)
            {
                _loggerService.LogException(ex);
                return BadRequest("One or more required parameters are null.");
            }
            catch (InvalidOperationException ex)
            {
                _loggerService.LogException(ex);
                return Conflict("Operation not allowed due to current object state.");
            }
            catch (Exception ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An unexpected error occurred while retrieving the data points.");
            }
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
            try
            {
                User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(id, HttpContext, e => e.DataPoints);
                List<UserRankingModel> rankingList = await _projectService.GetProjectRankingList(project);
                _loggerService.LogInformation(user.Id, $"Ranking overview for project {id} was retrieved.");
                return Ok(rankingList);
            }
            catch (ArgumentNullException ex)
            {
                _loggerService.LogException(ex);
                return BadRequest("One or more required parameters are null.");
            }
            catch (InvalidOperationException ex)
            {
                _loggerService.LogException(ex);
                return Conflict("Operation not allowed due to current object state.");
            }
            catch (Exception ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An unexpected error occurred while retrieving the data points.");
            }
        }
    }
}
