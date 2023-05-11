using d_lama_service.Repositories;
using Microsoft.AspNetCore.Mvc;
using Data.ProjectEntities;
using d_lama_service.Models.ProjectModels;
using d_lama_service.Models;
using Data;
using Microsoft.AspNetCore.Authorization;
using d_lama_service.Middleware;
using System.Net;
using System.Linq.Expressions;

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
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor of the ProjectController.
        /// </summary>
        public ProjectController(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves a list of all projects.
        /// </summary>
        /// <returns> A list of projects or Null if there are no projects at all. </returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _unitOfWork.ProjectRepository.GetAllAsync());
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
            User user = await GetAuthenticatedUserAsync();
            return Ok(await _unitOfWork.ProjectRepository.FindAsync(e => e.OwnerId == user.Id));
        }

        /// <summary>
        /// Retrieves a project with a given project ID.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await GetAuthenticatedUserAsync();
            var project = await _unitOfWork.ProjectRepository.GetDetailsAsync(id, e => e.Labels, e => e.DataPoints);
            
            if (project == null)
            {
                return NotFound();
            }

            var dataPointsIds = project.DataPoints.Select(e => e.Id).ToList();
            var labeledFromUser = await _unitOfWork.LabeledDataPointRepository.FindAsync(e => e.UserId == user.Id && dataPointsIds.Contains(e.DataPointId));

            return Ok(new DetailedProjectModel(project, dataPointsIds.Count, labeledFromUser.Count()));
        }

        /// <summary>
        /// Creates a new project with the data passed in the request body.
        /// </summary>
        /// <param name="projectForm"> The project form containing all needed information to create a project. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [AdminAuthorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProjectModel projectForm)
        {
            User user = await GetAuthenticatedUserAsync();

            var nameExists = (await _unitOfWork.ProjectRepository.FindAsync(e => e.Name == projectForm.ProjectName)).Any();
            if (nameExists)
            {
                return BadRequest("A project with this name has already been created.");
            }

            var project = new Project(projectForm.ProjectName, projectForm.Description);
            user.Projects.Add(project);
            foreach (var label in projectForm.Labels) 
            {
                project.Labels.Add(new Label(label.Name, label.Description));
            }

            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

            return Created(nameof(Get),project.Id);
        }

        /// <summary>
        /// Edits a new project with the data passed in the request body.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <param name="projectForm"> The project form containing all needed information to edit a project. </param>
        /// <returns> The updated project. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromBody] EditProjectModel projectForm)
        {
            var project = await GetProjectWithOwnerCheckAsync(id, e => e.Labels);

            var labeSetChanges = projectForm.LabeSetChanges;
            if (labeSetChanges != null)
            { 
                foreach (var change in labeSetChanges) 
                {
                    var label = project.Labels.Where(e => e.Id == change.Id).FirstOrDefault();
                    if (label == null) 
                    {
                        return NotFound($"Label with id {change.Id} not found.");
                    }
                    label.Name = change.Name ?? label.Name;
                    label.Description = change.Description ?? label.Description;
                }
            }

            project.Name = projectForm.Name ?? project.Name;
            project.Description = projectForm.Description ?? project.Description;

            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

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
            var project = await GetProjectWithOwnerCheckAsync(id);

            // cascade deletes children
            _unitOfWork.ProjectRepository.Delete(project);
            await _unitOfWork.SaveAsync();
            
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
        [HttpPost]
        [Route("{id:int}/Labels/")]
        public async Task<IActionResult> AddLabels(int id, [FromBody] LabelSetModel[] newLabels) 
        {
            var project = await GetProjectWithOwnerCheckAsync(id);

            foreach (var label in newLabels) 
            {
                project.Labels.Add(new Label(label.Name, label.Description));
            }

            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();
            
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
        [HttpDelete]
        [Route("{id:int}/Labels/{labelId:int}")]
        public async Task<IActionResult> RemoveLabel(int id, int labelId)
        {
            await GetProjectWithOwnerCheckAsync(id);

            var label = await _unitOfWork.LabelRepository.GetAsync(labelId);
            if (label == null || label.ProjectId != id) 
            {
                return NotFound();
            }

            // TODO: check if label already used and if so prohibit deletion
            _unitOfWork.LabelRepository.Delete(label);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        /// <summary>
        /// Gets a project with checking if the user is owner of the project. 
        /// </summary>
        /// <param name="projectId"> The id of the project. </param>
        /// <returns> The found project. </returns>
        /// <exception cref="RESTException"> Throws Rest Excetption if project is not found or the current user is not the owner. </exception>
        private async Task<Project> GetProjectWithOwnerCheckAsync(int projectId, params Expression<Func<Project, object>>[] includes) 
        {
            Project? project;
            if (includes.Any())
            {
                project = await _unitOfWork.ProjectRepository.GetDetailsAsync(projectId, includes);
            }
            else 
            {
                project = await _unitOfWork.ProjectRepository.GetAsync(projectId);
            }

            if (project == null)
            {
                throw new RESTException(HttpStatusCode.NotFound, $"Project with id {projectId} does not exist.");
            }

            var user = await GetAuthenticatedUserAsync();
            if (project.Owner != user)
            {
                throw new RESTException(HttpStatusCode.Unauthorized, $"Only the owner of the project can modify it.");
            }

            return project;
        }

        private async Task<User> GetAuthenticatedUserAsync()
        {
            var userId = int.Parse(HttpContext.User.FindFirst(Tokenizer.UserIdClaim)?.Value!); // on error throw
            return (await _unitOfWork.UserRepository.GetAsync(userId))!;
        }
    }
}
