using d_lama_service.Repositories;
using Microsoft.AspNetCore.Mvc;
using Data.ProjectEntities;
using d_lama_service.Models.ProjectModels;
using d_lama_service.Models;
using Data;
using Microsoft.AspNetCore.Authorization;
using d_lama_service.Attributes;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Runtime.CompilerServices;
using d_lama_service.DataProcessing;

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
            return Ok(await _unitOfWork.ProjectRepository.FindAsync(e => e.Owner == user));
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

            var nameExists = (await _unitOfWork.ProjectRepository.FindAsync(e => e.Name == projectForm.ProjectName)).Any();
            if (nameExists)
            {
                return BadRequest("A project with this name has already been created.");
            }

            // for testing, this should be done after files have been uploaded
            string testLabelEntry = "this is the first label";
            var label = new Label(testLabelEntry);

            var project = new Project(projectForm.ProjectName, projectForm.Description);

            user.Projects.Add(project);
            project.Labels.Add(label);
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
            if (project.Owner != user) 
            {
                return Unauthorized("Only the Owner of a project can modify it!");
            }

            project.Name = projectForm.ProjectName ?? project.Name;
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
            if (project.Owner != user)
            {
                return Unauthorized("Only the Owner of a project can modify it!");
            }

            // cascade deletes children
            _unitOfWork.ProjectRepository.Delete(project);
            await _unitOfWork.SaveAsync();
            
            return Ok();
        }

        /// <summary>
        /// Uploads a textual dataset and assigns the content to a given project.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [AdminAuthorize]
        [HttpPost("{projectId:int}/UploadText")]
        public async Task<IActionResult> UploadText(int projectId, IFormFile uploadedFile)
        {
            // Check if the project exists
            var project = await _unitOfWork.ProjectRepository.GetAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            // Check if project, already has a data set file uploaded

            // Check if a uploadedFile was uploaded
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            // check malware with library - not yet - first discuss which tool to use

            // check if uploadedFile in supported format
            DataSetReader dataSetReader = new DataSetReader(uploadedFile);

            if (!dataSetReader.IsValidFormat())
            {
                // The extension is invalid ... discontinue processing the uploadedFile
                return BadRequest("The uploaded file is not supported. Supported file extensions are ...");
            }

            // validate data format

            // read data into database
            ;
            if (!await dataSetReader.ReadFileAsync(project, _unitOfWork))
            {
                // The file could not be loaded to the database.
                return BadRequest("The file could not be loaded to the database.");
            }

            // update database metadata

            // return OK to user

            // storing the uploadedFile on disk and saving it in the DB should be done in a single transaction

            // in the DB we should only have the name of the uploadedFile and not the entire path


            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        /// <summary>
        /// Uploads a set of images and assigns them to a given project.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [AdminAuthorize]
        [HttpPost("{projectId:int}/UploadImages")]
        public async Task<IActionResult> UploadImages(int projectId, IFormFile uploadedFile)
        {
            // Check if the project exists
            var project = await _unitOfWork.ProjectRepository.GetAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            // Check if a uploadedFile was uploaded
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            // check malware with library - not yet - first discuss which tool to use

            // check if uploadedFile in supported format
            string[] permittedExtensions = { ".zip" };
            var fileExt = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExt) || !permittedExtensions.Contains(fileExt))
            {
                // The extension is invalid ... discontinue processing the uploadedFile
                return BadRequest("The uploaded file is not supported. Supported file extensions are ...");
            }

            // check if uploadedFile already exists

            // if exists - exit or update ?
            // if does not exist - determine uploadedFile name - unique and not from user 
            string myUniqueFileName = string.Format(@"{0}.txt", Guid.NewGuid());
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName);
            // - make sure the uploadedFile name is unique by checking that such a name does not exist

            const string STORAGE_DIR = "some-dir";

            // storage for the uploadedFile should not be the same as the project - e.g /some-other-dir/project-id/data-name
            string[] storagePath = { STORAGE_DIR, projectId.ToString() };
            var storageDir = Path.Join(storagePath);
            Directory.CreateDirectory(storageDir);

            // store uploadedFile on disk 
            var filePath = Path.Combine(myUniqueFileName);
            using (var stream = System.IO.File.Create(filePath))
            {
                await uploadedFile.CopyToAsync(stream);
            }

            // update database entry

            // return OK to user

            // storing the uploadedFile on disk and saving it in the DB should be done in a single transaction

            // in the DB we should only have the name of the uploadedFile and not the entire path

            // Unzip the uploaded uploadedFile and store its contents locally

            /*
            var extractPath = Path.Combine(storageDir, "extracted");
            Directory.CreateDirectory(extractPath);

            using (ZipArchive archive = new ZipArchive(new FileStream(filePath, FileMode.Open)))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Check if the entry is a uploadedFile
                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        // Check if the uploadedFile is a jpg, png, or csv uploadedFile
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

            // Update the project's metadata to include the extracted files

            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();
            */
            return Ok();
        }

        private async Task<User> GetAuthenticatedUserAsync()
        {
            var userId = int.Parse(HttpContext.User.FindFirst(Tokenizer.UserIdClaim)?.Value!); // on error throw
            return (await _unitOfWork.UserRepository.GetAsync(userId))!;
        }
    }
}
