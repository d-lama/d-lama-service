using d_lama_service.Repositories;
using Microsoft.AspNetCore.Mvc;
using Data.ProjectEntities;
using d_lama_service.Models.ProjectModels;
using d_lama_service.Models;
using Data;
using Microsoft.AspNetCore.Authorization;
using d_lama_service.Attributes;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

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

            string labelSetName = projectForm.ProjectName + " label set";
            var labelSet = new LabelSet(labelSetName);

            var project = new Project(projectForm.ProjectName, projectForm.Description, user.Id);
            project.DataPointSets.Add(dataSet);
            project.LabelSets.Add(labelSet);
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
        [AdminAuthorize]
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromBody] EditProjectModel projectForm)
        {        
            var project = await _unitOfWork.ProjectRepository.GetAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            var user = await GetAuthenticatedUserAsync();
            if (project.OwnerId != user.Id) 
            {
                return Unauthorized("Only the Owner of a project can modify it!");
            }

            project.ProjectName = projectForm.ProjectName ?? project.ProjectName;
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
        [AdminAuthorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _unitOfWork.ProjectRepository.GetAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var user = await GetAuthenticatedUserAsync();
            if (project.OwnerId != user.Id)
            {
                return Unauthorized("Only the Owner of a project can modify it!");
            }

            // cascade deletes children
            _unitOfWork.ProjectRepository.Delete(project);
            await _unitOfWork.SaveAsync();
            
            return Ok();
        }

        /// <summary>
        /// Uploads a file and assigns the content to a given project.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [AdminAuthorize]
        [HttpPost("{id:int}/UploadDataSet")]
        public async Task<IActionResult> Upload(int projectId, IFormFile file)
        {
            // Check if the project exists
            var project = await _unitOfWork.ProjectRepository.GetAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            // Check if a file was uploaded
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded");
            }

            // Check if the file is a zip archive
            if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("The uploaded file must be a zip archive");
            }

            // Create a unique filename for the uploaded file
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // Create a directory to store the uploaded file
            var directoryPath = Path.Combine(_environment.WebRootPath, "uploads", projectId.ToString());
            Directory.CreateDirectory(directoryPath);

            // Save the uploaded file to the directory
            var filePath = Path.Combine(directoryPath, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Unzip the uploaded file and store its contents locally
            var extractPath = Path.Combine(directoryPath, "extracted");
            Directory.CreateDirectory(extractPath);

            using (ZipArchive archive = new ZipArchive(new FileStream(filePath, FileMode.Open)))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Check if the entry is a file
                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        // Check if the file is a jpg, png, or csv file
                        if (!entry.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                            && !entry.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                            && !entry.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                        {
                            return BadRequest("The zip archive contains invalid files");
                        }

                        string extractFilePath = Path.Combine(extractPath, entry.FullName);

                        if (entry.FullName.EndsWith("/"))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(extractFilePath));
                            continue;
                        }

                        using (Stream entryStream = entry.Open())
                        using (FileStream extractStream = new FileStream(extractFilePath, FileMode.Create))
                        {
                            await entryStream.CopyToAsync(extractStream);
                        }
                    }
                }
            }

            // Check if the zip archive contains at least one image or csv file
            if (!Directory.EnumerateFiles(extractPath, "*.jpg", SearchOption.AllDirectories).Any()
                && !Directory.EnumerateFiles(extractPath, "*.png", SearchOption.AllDirectories).Any()
                && !Directory.EnumerateFiles(extractPath, "*.csv", SearchOption.AllDirectories).Any())
            {
                return BadRequest("The zip archive must contain at least one image or csv file");
            }

            // Update the project's metadata to include the extracted files
            project.ExtractedPath = extractPath;
            _unitOfWork.ProjectRepository.Update(project);
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
