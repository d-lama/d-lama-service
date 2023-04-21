using d_lama_service.DataProcessing;
using d_lama_service.Middleware;
using d_lama_service.Models.ProjectModels;
using d_lama_service.Repositories;
using Data.ProjectEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace d_lama_service.Controllers
{
    /// <summary>
    /// Data point controller handles the requests related to a data point.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DataPointController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor of the DataPointController.
        /// </summary>
        public DataPointController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves a list of all data points related to a project.
        /// </summary>
        /// <param name="projectId"> The ID of the project. </param>
        /// <returns> A list of data points or Null if there are no data points at all.</returns>
        [HttpGet]
        [Route("{projectId:int}")]
        public async Task<IActionResult> GetAll(int projectId)
        {
            var textDataPoints = await _unitOfWork.TextDataPointRepository.FindAsync(e => e.ProjectId == projectId);
            return Ok(textDataPoints);
        }

        /// <summary>
        /// Retrieves a data point related to a project by its index.
        /// </summary>
        /// <param name="projectId"> The ID of the project. </param>
        /// <param name="dataPointIndex"> The index of the data point. </param>
        /// <returns> A da</returns>
        [HttpGet]
        [Route("{projectId:int}/{dataPointIndex:int}")]
        public async Task<IActionResult> Get(int projectId, int dataPointIndex)
        {
            var textDataPoints = await _unitOfWork.TextDataPointRepository.FindAsync(e => e.ProjectId == projectId && e.DataPointIndex == dataPointIndex);
            return Ok(textDataPoints);
        }

        /// <summary>
        /// Uploads a textual dataset and assigns the content to a given project.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [AdminAuthorize]
        [HttpPost("{projectId:int}/UploadTextDataPoints")]
        public async Task<IActionResult> UploadTextDataPoints(int projectId, IFormFile uploadedFile)
        {
            // Check if the project exists
            var project = await _unitOfWork.ProjectRepository.GetAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            // Check if project, already has a data set file uploaded, or just append? Important for DataPointIndex
            // Maybe add a PUT endpoint for overwriting exisiting data

            // Check if a uploadedFile was uploaded
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            // TODO: check malware with library - not yet - first discuss which tool to use

            // check if uploadedFile in supported format
            DataSetReader dataSetReader = new DataSetReader();
            if (!dataSetReader.IsValidFormat(uploadedFile))
            {
                // The extension is invalid ... discontinue processing the uploadedFile
                return BadRequest("The uploaded file is not supported. Supported file extensions are .txt, .csv, .json");
            }

            // TODO: validate data format, header?

            // read data into database
            ICollection<string> textDataPoints = await dataSetReader.ReadFileAsync(uploadedFile);

            if (textDataPoints == null || textDataPoints.Count == 0)
            {
                // The file could not be loaded to the database.
                return BadRequest("The file could not be read or is empty.");
            }

            var presentDataPoints = await _unitOfWork.TextDataPointRepository.FindAsync(e => e.ProjectId == project.Id);
            var index = presentDataPoints.Count();
            foreach (var textDataPoint in textDataPoints)
            {
                project.TextDataPoints.Add(CreateTextDataPoint(textDataPoint, index));
                index++;
            }

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
        [HttpPost("{projectId:int}/UploadImageDataPoints")]
        public async Task<IActionResult> UploadImageDataPoints(int projectId, IFormFile uploadedFile)
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
        private TextDataPoint CreateTextDataPoint(string content, int row)
        {
            var dataPoint = new TextDataPoint(content, row);
            _unitOfWork.TextDataPointRepository.Update(dataPoint);
            return dataPoint;
        }

        /// <summary>
        /// Edits a text data point with a given project ID and data point index.
        /// </summary>
        /// <param name="projectId"> The project ID. </param>
        /// <param name="dataPointIndex"> The data point index. </param>
        /// <param name="dataPointForm"> The model with the updated fields. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400 or 404. </returns>
        [AdminAuthorize]
        [HttpPatch("{projectId:int}/EditTextDataPoint/{dataPointIndex:int}")]
        public async Task<IActionResult> EditTextDataPoint(int projectId, int dataPointIndex, [FromBody] EditTextDataPointModel dataPointForm)
        {
            var textDataPoints = await _unitOfWork.TextDataPointRepository
               .FindAsync(e => e.ProjectId == projectId && e.DataPointIndex == dataPointIndex);

            if (!textDataPoints.Any())
            {
                return NotFound("Data point not found.");
            }

            var textDataPoint = textDataPoints.First();

            textDataPoint.Content = dataPointForm.Content ?? textDataPoint.Content;

            _unitOfWork.TextDataPointRepository.Update(textDataPoint);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        /// <summary>
        /// Deletes all textual datapoints of a projact with a given project ID.
        /// </summary>
        /// <param name="projectId"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [AdminAuthorize]
        [HttpDelete]
        [Route("{projectId:int}/DeleteTextDataPoints")]
        public async Task<IActionResult> DeleteAllTextDataPoints(int projectId)
        {
            // TODO: check owner
            var textDataPoints = await _unitOfWork.TextDataPointRepository.FindAsync(e => e.ProjectId == projectId);

            if (!textDataPoints.Any())
            {
                return NotFound("No data points found for this project.");
            }

            foreach (var textDataPoint in textDataPoints)
            {
                _unitOfWork.TextDataPointRepository.Delete(textDataPoint);
            }
            
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        /// <summary>
        /// Deletes all datapoints of a projact with a given project ID.
        /// </summary>
        /// <param name="projectId"> The project ID. </param>
        /// <param name="startIndex"> The start of index range (inclusive). </param>
        /// <param name="endIndex"> The end of index range (inclusive). </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [AdminAuthorize]
        [HttpDelete]
        [Route("{projectId:int}/DeleteTextDataPoints/{startIndex:int}/{endIndex:int}")]
        public async Task<IActionResult> DeleteTextDataPointRange(int projectId, int startIndex, int endIndex)
        {
            // TODO: check owner
            var textDataPoints = await _unitOfWork.TextDataPointRepository
                .FindAsync(e => e.ProjectId == projectId && e.DataPointIndex >= startIndex && e.DataPointIndex <= endIndex);

            if (!textDataPoints.Any())
            {
                return NotFound("No data points found for this project and index range.");
            }

            foreach (var textDataPoint in textDataPoints)
            {
                _unitOfWork.TextDataPointRepository.Delete(textDataPoint);
            }

            // TODO: maybe reindex TextDataPoints
            await _unitOfWork.SaveAsync();

            return Ok();
        }
    }
}
