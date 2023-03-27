using d_lama_service.Repositories;
using Microsoft.AspNetCore.Mvc;
using Data.ProjectEntities;
using d_lama_service.Models.ProjectModels;
using d_lama_service.Models;
using Data;
using Microsoft.AspNetCore.Authorization;

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
        /// <returns> A list all projects. </returns>
        [Authorize]
        [HttpGet]
        [Route("getAllProjects")]
        public async Task<IActionResult> GetAllProjects()
        {
             var projectList = await _unitOfWork.ProjectRepository.GetAllAsync();

            if (projectList == null)
            {
                return NotFound();
            }
            return Ok(projectList);
        }

        /// <summary>
        /// Retrieves a list of projects by a given admin user.
        /// </summary>
        /// <returns> A list of the projects created by logged in admin user. </returns>
        [Authorize]
        [HttpGet]
        [Route("getOwnerProjects")]
        public async Task<IActionResult> GetOwnerProjects()
        {
            User user = await GetAuthenticatedUserAsync();
            var userId = user.Id;
            if (!user.IsAdmin)
            {
                return BadRequest("This action is restricted to admin users.");
            }
            var projectList = await _unitOfWork.ProjectRepository.FindAsync(e => e.OwnerId == user.Id);

            return Ok(projectList);
        }

        /// <summary>
        /// Retrieves a project with a given project ID.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _unitOfWork.ProjectRepository.GetAsync(id);

            if (project == null) 
            {
                return NotFound();
            }
            return Ok(project);
        }

        /// <summary>
        /// Creates a new project with the data passed in the request body.
        /// </summary>
        /// <param name="projectForm"> The project form containing all needed information to create a project. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectModel projectForm)
        {
            User user = await GetAuthenticatedUserAsync();
            var userId = user.Id;
            if (!user.IsAdmin)
            {
                return BadRequest("This action is restricted to admin users.");
            }

            var nameExists = (await _unitOfWork.ProjectRepository.FindAsync(e => e.ProjectName == projectForm.ProjectName)).Any();
            if (nameExists)
            {
                return BadRequest("A project with this name has already been created.");
            }

            string dataSetName = projectForm.ProjectName + " data set";
            var dataSet = new DataPointSet(dataSetName);
            _unitOfWork.DataPointSetRepository.Add(dataSet);
            await _unitOfWork.SaveAsync();

            string labelSetName = projectForm.ProjectName + " label set";
            var labelSet = new LabelSet(labelSetName);
            _unitOfWork.LabelSetRepository.Add(labelSet);
            await _unitOfWork.SaveAsync();

            var project = new Project(projectForm.ProjectName, projectForm.Description, userId, dataSet.Id, labelSet.Id);
            _unitOfWork.ProjectRepository.Add(project);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        /// <summary>
        /// Edits a new project with the data passed in the request body.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <param name="projectForm"> The project form containing all needed information to edit a project. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [Authorize]
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] EditProjectModel projectForm)
        {
            User user = await GetAuthenticatedUserAsync();
            if (!user.IsAdmin)
            {
                return BadRequest("This action is restricted to admin users.");
            }
            var project = await _unitOfWork.ProjectRepository.GetAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            project.ProjectName = projectForm.ProjectName ?? project.ProjectName;
            project.Description = projectForm.Description ?? project.Description;

            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        /// <summary>
        /// Deletes a project with a given project ID.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            User user = await GetAuthenticatedUserAsync();
            if (!user.IsAdmin)
            {
                return BadRequest("This action is restricted to admin users.");
            }

            var project = await _unitOfWork.ProjectRepository.GetAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            var dataPointSet = await _unitOfWork.DataPointSetRepository.GetAsync(project.DataSetId);
            if (dataPointSet != null)
            {
                _unitOfWork.DataPointSetRepository.Delete(dataPointSet);
            }
            var labelSet = await _unitOfWork.LabelSetRepository.GetAsync(project.LabelSetId);
            if (labelSet != null)
            {
                _unitOfWork.LabelSetRepository.Delete(labelSet);
            }

            _unitOfWork.ProjectRepository.Delete(project);
            
            await _unitOfWork.SaveAsync();
            
            return Ok();
        }
        private async Task<User> GetAuthenticatedUserAsync()
        {
            var userId = int.Parse(HttpContext.User.FindFirst(Tokenizer.UserIdClaim)?.Value!); // on error throw
            return (await _unitOfWork.UserRepository.GetAsync(userId))!;
        }
    }
}
