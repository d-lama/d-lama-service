using d_lama_service.Repositories;
using Microsoft.AspNetCore.Mvc;
using Data.ProjectEntities;
using d_lama_service.Models.ProjectModels;
using d_lama_service.Models;
using Data;
using Microsoft.AspNetCore.Authorization;
using d_lama_service.Attributes;

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
        [Route("GetAllProjects")]
        public async Task<IActionResult> GetAllProjects()
        {
            return Ok(await _unitOfWork.ProjectRepository.GetAllAsync());
        }

        /// <summary>
        /// Retrieves a list of projects by a given admin user.
        /// </summary>
        /// <returns> A list of the projects created by logged in admin user. </returns>
        [AdminAuthorize]
        [HttpGet]
        [Route("GetAdminProjects")]
        public async Task<IActionResult> GetAdminProjects()
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
        [AdminAuthorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProjectModel projectForm)
        {
            User user = await GetAuthenticatedUserAsync();

            var nameExists = (await _unitOfWork.ProjectRepository.FindAsync(e => e.ProjectName == projectForm.ProjectName)).Any();
            if (nameExists)
            {
                return BadRequest("A project with this name has already been created.");
            }

            string dataSetName = projectForm.ProjectName + " data set";
            var dataSet = new DataPointSet(dataSetName);
            _unitOfWork.DataPointSetRepository.Update(dataSet);
            // await _unitOfWork.SaveAsync();

            string labelSetName = projectForm.ProjectName + " label set";
            var labelSet = new LabelSet(labelSetName);
            _unitOfWork.LabelSetRepository.Update(labelSet);
            // await _unitOfWork.SaveAsync();

            var project = new Project(projectForm.ProjectName, projectForm.Description, user.Id);
            // NullReferenceException: Object reference not set to an instance of an object
            project.DataPointSets.Add(dataSet);
            project.LabelSets.Add(labelSet);
            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        /// <summary>
        /// Edits a new project with the data passed in the request body.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <param name="projectForm"> The project form containing all needed information to edit a project. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [AdminAuthorize]
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] ProjectModel projectForm)
        {        
            var project = await _unitOfWork.ProjectRepository.GetAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            // no null in projectForm because of required annotation in model
            project.ProjectName = projectForm.ProjectName;
            project.Description = projectForm.Description;

            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        /// <summary>
        /// Deletes a project with a given project ID.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
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

            // cascade deletes children
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
