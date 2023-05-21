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
using d_lama_service.Models.UserModels;

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
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Constructor of the ProjectController.
        /// </summary>
        /// <param name="unitOfWork"> The unitOfWork for handling db access. </param>
        /// <param name="environment"> The web hosting environment. </param>
        public ProjectController(IUnitOfWork unitOfWork, IWebHostEnvironment environment) 
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        /// <summary>
        /// Retrieves a list of all projects.
        /// </summary>
        /// <returns> A list of projects or Null if there are no projects at all. </returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projectList = new List<DetailedProjectModel>();
            var user = await GetAuthenticatedUserAsync();
            var projects = await _unitOfWork.ProjectRepository.GetAllAsync();
            foreach (var project in projects) 
            {
                var detailedProject = await _unitOfWork.ProjectRepository.GetDetailsAsync(project.Id, e => e.DataPoints);
                var dataPointsIds = project.DataPoints.Select(e => e.Id).ToList();
                var labeledFromUser = await _unitOfWork.LabeledDataPointRepository.FindAsync(e => e.UserId == user.Id && dataPointsIds.Contains(e.DataPointId));
                projectList.Add(new DetailedProjectModel(project, dataPointsIds.Count, labeledFromUser.Count()));
            }
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

            Project? project;
            string? projectDirectoryPath;

            if (projectForm.DataType == ProjectDataType.Image)
            {
                var webRootPath = _environment.WebRootPath.ToString();
                projectDirectoryPath = Path.Combine(webRootPath, "project_files");
                project = new Project(projectForm.ProjectName, projectForm.Description, projectDirectoryPath);
            } else if (projectForm.DataType == ProjectDataType.Text) {
                project = new Project(projectForm.ProjectName, projectForm.Description);
            } else
            {
                return BadRequest("Unsupported data type. The following data types are supported: text, image.");
            }

            user.Projects.Add(project);
            foreach (var label in projectForm.Labels) 
            {
                project.Labels.Add(new Label(label.Name, label.Description));
            }

            // save changes needed in order to get Id
            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

            if (project.DataType == ProjectDataType.Image)
            {
                await CreateFileDirectory(project);
            }

            var createdResource = new { id = project.Id };
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
            project.UpdateDate = DateTime.UtcNow;

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

            if (project.DataType == ProjectDataType.Image)
            {
                Directory.Delete(project.StoragePath, true);
            }

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
        [HttpPost("{id:int}/Labels/")]
        public async Task<IActionResult> AddLabels(int id, [FromBody] LabelSetModel[] newLabels) 
        {
            var project = await GetProjectWithOwnerCheckAsync(id, e => e.Labels);

            foreach (var label in newLabels) 
            {
                if (project.Labels.Select(e => e.Name).Contains(label.Name)) 
                {
                    await _unitOfWork.DisposeAsync();
                    return BadRequest($"A label with the name '{label.Name}' does already exists. Please use another name.");
                }
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
        [HttpDelete("{id:int}/Labels/{labelId:int}")]
        public async Task<IActionResult> RemoveLabel(int id, int labelId)
        {
            await GetProjectWithOwnerCheckAsync(id);

            var label = await _unitOfWork.LabelRepository.GetAsync(labelId);
            if (label == null || label.ProjectId != id) 
            {
                return NotFound();
            }

            var labeledData = await _unitOfWork.LabeledDataPointRepository.FindAsync(e => e.LabelId == labelId);
            if (labeledData.Any()) 
            {
                return BadRequest("Label was already used and cannot be deleted therefore.");
            }

            _unitOfWork.LabelRepository.Delete(label);
            await _unitOfWork.SaveAsync();

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
        public async Task<IActionResult> GetUserRanking(int id) 
        {
            var project = await GetProjectWithOwnerCheckAsync(id, e => e.DataPoints);
            var dataPointIds = project.DataPoints.Select(e => e.Id);
            if (dataPointIds == null || !dataPointIds.Any()) 
            {
                return BadRequest("You need to add DataPoints first, before you can get a ranking!");
            }

            var users = await _unitOfWork.UserRepository.GetAllAsync();

            var ranking = new List<UserRankingModel>();

            foreach (var user in users ) 
            {
                var labeledPointsCount = (await _unitOfWork.LabeledDataPointRepository.FindAsync(e => e.UserId == user.Id && dataPointIds.Contains(e.DataPointId))).Count();
                var percentage = (float)labeledPointsCount / (float)dataPointIds.Count();
                ranking.Add(new UserRankingModel(user.Id, user.FirstName + " " + user.LastName, percentage));
            }
            return Ok(ranking.OrderByDescending(e => e.Percentage));
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

        /// <summary>
        /// Creates a project directory in the file storage path. 
        /// </summary>
        /// <param name="project"> The project. </param>
        private async Task CreateFileDirectory(Project project)
        {
            var projectDirectoryPath = Path.Combine(project.StoragePath, $"project_{project.Id}");
            Directory.CreateDirectory(projectDirectoryPath);
            project.StoragePath = projectDirectoryPath;

            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();
        }

        private async Task<User> GetAuthenticatedUserAsync()
        {
            var userId = int.Parse(HttpContext.User.FindFirst(Tokenizer.UserIdClaim)?.Value!); // on error throw
            return (await _unitOfWork.UserRepository.GetAsync(userId))!;
        }
    }
}
