using d_lama_service.DataProcessing;
using d_lama_service.Middleware;
using d_lama_service.Models;
using d_lama_service.Models.ProjectModels;
using d_lama_service.Repositories;
using Data;
using Data.ProjectEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Net;

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
        /// <returns> 200 with a list of data points or 404 if there are no data points at all. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet]
        [Route("{projectId:int}")]
        public async Task<IActionResult> GetAllTextDataPointsAsync(int projectId)
        {
            var project = await GetProjectAsync(projectId);

            var textDataPoints = await _unitOfWork.TextDataPointRepository.FindAsync(e => e.ProjectId == projectId);
            if (!textDataPoints.Any())
            {
                return NotFound("No data points found for this project.");
            }
            return Ok(textDataPoints);
        }

        /// <summary>
        /// Retrieves a the number of data points related to a project.
        /// </summary>
        /// <param name="projectId"> The ID of the project. </param>
        /// <returns> The number of text data points. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet]
        [Route("{projectId:int}/GetNumberOfTextDataPointsAsync")]
        public async Task<IActionResult> GetNumberOfTextDataPointsAsync(int projectId)
        {
            var project = await GetProjectAsync(projectId);

            // TODO: maybe define count in repository and not get whole list here
            var textDataPoints = await _unitOfWork.TextDataPointRepository.FindAsync(e => e.ProjectId == projectId);
            return Ok(textDataPoints.Count());
        }

        /// <summary>
        /// Retrieves a data point related to a project by its index.
        /// </summary>
        /// <param name="projectId"> The ID of the project. </param>
        /// <param name="dataPointIndex"> The index of the data point. </param>
        /// <returns> 200 with a list of data points or 404 if there are no data points at all. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet]
        [Route("{projectId:int}/{dataPointIndex:int}")]
        public async Task<IActionResult> GetTextDataPointAsync(int projectId, int dataPointIndex)
        {
            var project = await GetProjectAsync(projectId);

            var textDataPoints = await _unitOfWork.TextDataPointRepository.FindAsync(e => e.ProjectId == projectId && e.DataPointIndex == dataPointIndex);
            if (!textDataPoints.Any())
            {
                return NotFound("No data point found for this project and index.");
            }
            var textDataPoint = textDataPoints.First();
            return Ok(new ReadTextDataPointModel(textDataPoint));
        }

        /// <summary>
        /// Retrieves a range of data point related to a project.
        /// </summary>
        /// <param name="projectId"> The ID of the project. </param>
        /// <param name="startIndex"> The start of index range (inclusive). </param>
        /// <param name="endIndex"> The end of index range (inclusive). </param>
        /// <returns> A da</returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet]
        [Route("{projectId:int}/{startIndex:int}/{endIndex:int}")]
        public async Task<IActionResult> GetTextDataPointRangeAsync(int projectId, int startIndex, int endIndex)
        {
            var project = await GetProjectAsync(projectId);

            var textDataPoints = await _unitOfWork.TextDataPointRepository
                .FindAsync(e => e.ProjectId == projectId && e.DataPointIndex >= startIndex && e.DataPointIndex <= endIndex);

            if (!textDataPoints.Any())
            {
                return NotFound("No data points found for this project and index range.");
            }
            return Ok(textDataPoints);
        }

        /// <summary>
        /// Creates a single textual data point and assigns it to a given project.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// /// <param name="dataPointForm"> The data point form. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpPost("{projectId:int}/CreateSingleTextDataPointAsync")]
        public async Task<IActionResult> CreateSingleTextDataPointAsync(int projectId, [FromBody] TextDataPointModel dataPointForm)
        {
            // Check if the project exists
            var project = await GetProjectWithOwnerCheckAsync(projectId);

            var index = await GetNextTextDataPointIndexAsync(project);
            var dataPoint = new TextDataPoint(dataPointForm.Content, index);
            project.TextDataPoints.Add(dataPoint);
            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

            var uri = nameof(GetTextDataPointAsync) + "/" + dataPoint.ProjectId + "/" + dataPoint.DataPointIndex;
            return Created(uri, dataPoint.Content);
        }

        /// <summary>
        /// Uploads a textual dataset and assigns the content to a given project.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpPost("{projectId:int}/UploadTextDataPointsAsync")]
        public async Task<IActionResult> UploadTextDataPointsAsync(int projectId, IFormFile uploadedFile)
        {
            // Check if the project exists
            var project = await GetProjectWithOwnerCheckAsync(projectId);

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

            var index = await GetNextTextDataPointIndexAsync(project);
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
        /// Edits a text data point with a given project ID and data point index.
        /// </summary>
        /// <param name="projectId"> The project ID. </param>
        /// <param name="dataPointIndex"> The data point index. </param>
        /// <param name="dataPointForm"> The model with the updated fields. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400 or 404. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpPatch("{projectId:int}/EditTextDataPointAsync/{dataPointIndex:int}")]
        public async Task<IActionResult> EditTextDataPointAsync(int projectId, int dataPointIndex, [FromBody] EditTextDataPointModel dataPointForm)
        {
            // Check if the project exists and if user is owner
            var project = await GetProjectWithOwnerCheckAsync(projectId);

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
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpDelete]
        [Route("{projectId:int}/DeleteTextDataPoints")]
        public async Task<IActionResult> DeleteAllTextDataPointsAsync(int projectId)
        {
            // Check if the project exists and if user is owner
            var project = await GetProjectWithOwnerCheckAsync(projectId);

            var textDataPoints = await _unitOfWork.TextDataPointRepository.FindAsync(e => e.ProjectId == projectId);

            if (!textDataPoints.Any())
            {
                return NotFound("No data points found for this project.");
            }

            foreach (var textDataPoint in textDataPoints)
            {
                _unitOfWork.TextDataPointRepository.Delete(textDataPoint);
            }
            
            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        /// <summary>
        /// Deletes all datapoints of a project with a given project ID.
        /// </summary>
        /// <param name="projectId"> The project ID. </param>
        /// <param name="startIndex"> The start of index range (inclusive). </param>
        /// <param name="endIndex"> The end of index range (inclusive). </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpDelete]
        [Route("{projectId:int}/DeleteTextDataPoints/{startIndex:int}/{endIndex:int}")]
        public async Task<IActionResult> DeleteTextDataPointRangeAsync(int projectId, int startIndex, int endIndex)
        {
            // Check if the project exists and if user is owner
            var project = await GetProjectWithOwnerCheckAsync(projectId);

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
            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        /// <summary>
        /// Returns the next DataPointIndex for a TextDataPoint. 
        /// </summary>
        /// <param name="project"> The project. </param>
        /// <returns> Next index. </returns>
        private async Task<int> GetNextTextDataPointIndexAsync(Project project)
        {
            var presentDataPoints = await _unitOfWork.TextDataPointRepository.FindAsync(e => e.ProjectId == project.Id);
            //  ?? 0 -> return 0 if the collection is empty
            int index = presentDataPoints.OrderByDescending(dp => dp.DataPointIndex).FirstOrDefault()?.DataPointIndex + 1 ?? 0;
            return index;
        }

        /// <summary>
        /// Creates a textual data point with given content and index. 
        /// </summary>
        /// <param name="content"> Data point content. </param>
        /// <param name="index"> Data point index. </param>
        /// <returns> The created text data point. </returns>
        private TextDataPoint CreateTextDataPoint(string content, int index)
        {
            var dataPoint = new TextDataPoint(content, index);
            _unitOfWork.TextDataPointRepository.Update(dataPoint);
            return dataPoint;
        }

        /// <summary>
        /// Gets a project with checking if it exists. 
        /// </summary>
        /// <param name="projectId"> The id of the project. </param>
        /// <returns> The found project. </returns>
        /// <exception cref="RESTException"> Throws Rest Excetption if project is not found. </exception>
        private async Task<Project> GetProjectAsync(int projectId)
        {
            Project? project = await _unitOfWork.ProjectRepository.GetAsync(projectId);
            if (project == null)
            {
                throw new RESTException(HttpStatusCode.NotFound, $"Project with id {projectId} does not exist.");
            }
            return project;
        }

        // TODO: refactor, find solution for GetProjectWithOwnerCheckAsync that is a duplicate of the one in ProjectController
        /// <summary>
        /// Gets a project with checking if the user is owner of the project. 
        /// </summary>
        /// <param name="projectId"> The id of the project. </param>
        /// <returns> The found project. </returns>
        /// <exception cref="RESTException"> Throws Rest Excetption if project is not found or the current user is not the owner. </exception>
        private async Task<Project> GetProjectWithOwnerCheckAsync(int projectId, params Expression<Func<Project, object>>[] includes)
        {
            Project project = await GetProjectAsync(projectId);

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
