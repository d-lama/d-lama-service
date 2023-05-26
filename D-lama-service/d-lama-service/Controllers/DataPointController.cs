using d_lama_service.Middleware;
using d_lama_service.Models.DataPointModels;
using d_lama_service.Models.ProjectModels;
using d_lama_service.Services;
using Data;
using Data.ProjectEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        private readonly IDataPointService _dataPointService;
        private readonly ISharedService _sharedService;
        private readonly ILoggerService _loggerService;

        /// <summary>
        /// Constructor of the DataPointController.
        /// </summary>
        public DataPointController(IDataPointService dataPointService, ISharedService sharedService, ILoggerService loggerService)
        {
            _dataPointService = dataPointService;
            _sharedService = sharedService;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Retrieves a list of all data points related to a project.
        /// </summary>
        /// <param name="projectId"> The ID of the project. </param>
        /// <returns> 200 with a list of data points or 404 if there are no data points at all. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet("{projectId:int}")]
        public async Task<IActionResult> GetAll(int projectId)
        {
            try
            {
                User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                List<ReadDataPointModel> dataPoints = await _dataPointService.GetDataPointsFromProjectAsync(projectId, user);
                _loggerService.LogInformation(user.Id, $"A list of all the data points for project {projectId} was successfully retrieved.");
                return Ok(dataPoints);
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
        /// Retrieves a the number of data points related to a project.
        /// </summary>
        /// <param name="projectId"> The ID of the project. </param>
        /// <returns> The number of text data points. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet("{projectId:int}/GetNumberOfDataPoints")]
        public async Task<IActionResult> GetCount(int projectId)
        {
            try
            {
                int count = await _dataPointService.GetDataPointsCountOfProjectAsync(projectId);
                _loggerService.LogInformation(-1, $"The number of data points for project {projectId} was successfully retrieved.");
                return Ok(count);
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
                return StatusCode(500, "An unexpected error occurred while retrieving the data point count.");
            }

        }

        /// <summary>
        /// Retrieves a data point related to a project by its index.
        /// </summary>
        /// <param name="projectId"> The ID of the project. </param>
        /// <param name="dataPointIndex"> The index of the data point. </param>
        /// <returns> 200 with the data point or 404 if not match found. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet("{projectId:int}/{dataPointIndex:int}")]
        public async Task<IActionResult> Get(int projectId, int dataPointIndex)
        {
            try
            {
                ProjectDataType dataType = await _dataPointService.GetProjectTypeAsync(projectId);
                if (dataType == ProjectDataType.Text)
                {
                    User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                    ReadTextDataPointModel textDatPoint = await _dataPointService.GetTextDataPointAsync(user, projectId, dataPointIndex);
                    _loggerService.LogInformation(user.Id, $"The data point with index {dataPointIndex} for project {projectId} was successfully retrieved.");
                    return Ok(textDatPoint);
                }
                (byte[] imageBytes, string contentType) = await _dataPointService.GetImageDataPointAsync(projectId, dataPointIndex);
                _loggerService.LogInformation(-1, $"The data point with index {dataPointIndex} for project {projectId} was successfully retrieved.");
                return File(imageBytes, contentType);
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
                return StatusCode(500, "An unexpected error occurred while retrieving the data point by its index.");
            }
        }

        /// <summary>
        /// Retrieves a range of data points related to a project.
        /// </summary>
        /// <param name="projectId"> The ID of the project. </param>
        /// <param name="startIndex"> The start index of DataPoint range (inclusive). </param>
        /// <param name="endIndex"> The end index of DataPoint range (inclusive). </param>
        /// <returns> The data points. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet("{projectId:int}/{startIndex:int}/{endIndex:int}")]
        public async Task<IActionResult> GetRange(int projectId, int startIndex, int endIndex)
        {
            try
            {
                User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                List<ReadDataPointModel> dataPoints = await _dataPointService.GetDataPointsFromProjectAsync(projectId, user, startIndex, endIndex);
                _loggerService.LogInformation(-1, $"The data points in range for project {projectId} were successfully retrieved.");
                return Ok(dataPoints);
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
                return StatusCode(500, "An unexpected error occurred while retrieving the data points in range.");
            }
        }

        /// <summary>
        /// Creates a single textual data point and assigns it to a given project.
        /// </summary>
        /// <param name="id"> The project ID. </param>
        /// /// <param name="dataPointForm"> The data point form. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpPost("{projectId:int}/CreateSingleTextDataPoint")]
        public async Task<IActionResult> CreateTextPoint(int projectId, [FromBody] TextDataPointModel dataPointForm)
        {
            try
            {
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
                int dataPointIndex = await _dataPointService.CreateTextDataPointAsync(project, dataPointForm);
                string uri = nameof(Get) + "/" + projectId + "/" + dataPointIndex;
                _loggerService.LogInformation(-1, $"Textual data point for project {projectId} was successfully created.");
                return Created(uri, dataPointIndex);
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
                return StatusCode(500, "An unexpected error occurred while creating the text data point.");
            }
        }

        /// <summary>
        /// Uploads a textual dataset and assigns the content to a given project.
        /// </summary>
        /// <param name="projectId"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpPost("{projectId:int}/UploadTextDataPoints")]
        public async Task<IActionResult> CreateMutlipleTextPoints(int projectId, IFormFile uploadedFile)
        {
            try
            {
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
                await _dataPointService.CreateTextDataPointsAsync(project, uploadedFile);
                _loggerService.LogInformation(-1, $"The dataset for project {projectId} was successfully uploaded.");
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
                return StatusCode(500, "An unexpected error occurred while creating multiple text data points.");
            }

        }

        /// <summary>
        /// Uploads a single image file and assigns it to a given project.
        /// </summary>
        /// <param name="projectId"> The project ID. </param>
        /// <param name="uploadedFile"> The image file. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpPost("{projectId:int}/UploadSingleImageDataPoint")]
        public async Task<IActionResult> CreateImagePoint(int projectId, IFormFile uploadedFile)
        {
            try
            {
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
                int dataPointIndex = await _dataPointService.CreateImageDataPointAsync(project, uploadedFile);
                string uri = nameof(Get) + "/" + projectId + "/" + dataPointIndex;
                _loggerService.LogInformation(-1, $"Image data point for project {projectId} was successfully created.");
                return Created(uri, dataPointIndex);
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
                return StatusCode(500, "An unexpected error occurred while creating the image data point.");
            }
        }

        /// <summary>
        /// Uploads an image dataset and assigns the content to a given project.
        /// </summary>
        /// <param name="projectId"> The project ID. </param>
        /// <param name="uploadedFile"> The compressed file containing the images. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpPost("{projectId:int}/UploadImageDataPoints")]
        public async Task<IActionResult> CreateMultipleImagePoints(int projectId, IFormFile uploadedFile)
        {
            try
            {
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
                await _dataPointService.CreateImageDataPointsAsync(project, uploadedFile);
                _loggerService.LogInformation(-1, $"The dataset for project {projectId} was successfully uploaded.");
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
                return StatusCode(500, "An unexpected error occurred while creating multiple image data points.");
            }

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
        [HttpPatch("{projectId:int}/EditTextDataPoint/{dataPointIndex:int}")]
        public async Task<IActionResult> EditTextPoint(int projectId, int dataPointIndex, [FromBody] EditTextDataPointModel dataPointForm)
        {
            try
            {
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
                await _dataPointService.UpdateTextDataPointAsync(project, dataPointIndex, dataPointForm);
                _loggerService.LogInformation(-1, $"The data point {dataPointIndex} of project {projectId} was successfully edited.");
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
                return StatusCode(500, "An unexpected error occurred while editing the text data point.");
            }
        }

        /// <summary>
        /// Edits an image data point with a given project ID and data point index.
        /// </summary>
        /// <param name="projectId"> The project ID. </param>
        /// <param name="dataPointIndex"> The data point index. </param>
        /// <param name="uploadedFile"> The updated file that will replace the currently stored one. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400 or 404. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpPatch("{projectId:int}/EditImageDataPoint/{dataPointIndex:int}")]
        public async Task<IActionResult> EditImagePoint(int projectId, int dataPointIndex, IFormFile uploadedFile)
        {
            try
            {
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
                await _dataPointService.UpdateImageDataPointAsync(project, dataPointIndex, uploadedFile);
                _loggerService.LogInformation(-1, $"The data point {dataPointIndex} of project {projectId} was successfully edited.");
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
                return StatusCode(500, "An unexpected error occurred while editing the image data point.");
            }
        }

        /// <summary>
        /// Deletes all data points of a projact with a given project ID.
        /// </summary>
        /// <param name="projectId"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 404. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpDelete]
        [Route("{projectId:int}/DeleteDataPoints")]
        public async Task<IActionResult> DeleteAll(int projectId)
        {
            try
            {
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
                await _dataPointService.DeleteDataPointsAsync(project);
                _loggerService.LogInformation(-1, $"All data points of project {projectId} were successfully deleted.");
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
                return StatusCode(500, "An unexpected error occurred while deleting all data points.");
            }
        }

        /// <summary>
        /// Deletes a range of data points of a project with a given project ID.
        /// </summary>
        /// <param name="projectId"> The project ID. </param>
        /// <param name="startIndex"> The start index of range (inclusive). </param>
        /// <param name="endIndex"> The end index of range (inclusive). </param>
        /// <returns> Statuscode 200 on success, else Statuscode 404. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpDelete("{projectId:int}/DeleteDataPoints/{startIndex:int}/{endIndex:int}")]
        public async Task<IActionResult> DeleteTextPointRange(int projectId, int startIndex, int endIndex)
        {
            try
            {
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
                await _dataPointService.DeleteDataPointsAsync(project, startIndex, endIndex);
                _loggerService.LogInformation(-1, $"The data points in range of project {projectId} were successfully deleted.");
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
                return StatusCode(500, "An unexpected error occurred while deleting the text data points in range.");
            }

        }

        /// <summary>
        /// Labels a datapoint.
        /// </summary>
        /// <param name="projectId"> The project id where the datapoint is belonging to. </param>
        /// <param name="dataPointIndex"> The data point index to be labeled. </param>
        /// <param name="labelId"> The label id used to label it. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpPost("{projectId:int}/LabelDataPoint/{dataPointIndex:int}")]
        public async Task<IActionResult> LabelPoint(int projectId, int dataPointIndex, [FromBody] int labelId)
        {
            try
            {
                User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                await _dataPointService.LabelDataPointsAsync(user, projectId, dataPointIndex, labelId);
                _loggerService.LogInformation(user.Id, $"Data point {dataPointIndex} of project {projectId} was successfully labeled.");
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
                return StatusCode(500, "An unexpected error occurred while labeling the data point.");
            }

        }

        /// <summary>
        /// Deletes a labeled datapoint entry. 
        /// </summary>
        /// <param name="projectId"> The project id where the datapoint is belonging to. </param>
        /// <param name="dataPointIndex"> The data point index to be deleted. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpDelete("{projectId:int}/LabelDataPoint/{dataPointIndex:int}")]
        public async Task<IActionResult> RemoveLabeledPoint(int projectId, int dataPointIndex)
        {
            try
            {
                User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                await _dataPointService.DeleteLabeledDataPointAsync(user, projectId, dataPointIndex);
                _loggerService.LogInformation(user.Id, $"The label of data point {dataPointIndex} of project {projectId} was successfully deleted.");
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
                return StatusCode(500, "An unexpected error occurred while removing the labeled data point.");
            }
        }

        /// <summary>
        /// Gets all information of the labeled datapoints of a project.
        /// </summary>
        /// <param name="projectId"> The id of the project. </param>
        /// <returns> Statuscode 200 with the list of information about the datapoints, if the project is found and the owner made the request, else statuscode 400. </returns>
        [AdminAuthorize]
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet("{projectId:int}/GetLabeledData")]
        public async Task<IActionResult> GetLabeledData(int projectId)
        {
            try
            {
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext, e => e.DataPoints, e => e.Labels);
                LabeledDataModel labeledData = await _dataPointService.GetLabeledDataForProjectAsync(project);
                _loggerService.LogInformation(-1, $"Information of the labeled datapoints of project {projectId} were successfully retrieved.");
                return Ok(labeledData);
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
                return StatusCode(500, "An unexpected error occurred while retrieving labeled data for the project.");
            }
        }

        /// <summary>
        /// Gets information about a specific datapoint of a project.
        /// </summary>
        /// <param name="projectId"> The id of the project. </param>
        /// <param name="dataPointIndex"> The index of the datapoint. </param>
        /// <returns> Statuscode 200 with the label details, if the request was made from the project owner and the project is found, else statuscode 400. </returns>
        [AdminAuthorize]
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet("{projectId:int}/GetLabeledData/{dataPointIndex:int}")]
        public async Task<IActionResult> GetLabeledPoint(int projectId, int dataPointIndex) 
        {
            try
            {
                Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext); // for owner check
                LabeledDataPointModel labeledDataPoint = await _dataPointService.GetLabeledDataPointForProject(project, dataPointIndex);
                _loggerService.LogInformation(-1, $"Information to data point {dataPointIndex} of project {projectId} was successfully retrieved.");
                return Ok(labeledDataPoint);
            }
            catch (Exception ex)
            {
                _loggerService.LogException(ex);
                return StatusCode(500, "An unexpected error occurred while retrieving the labeled data point.");
            }

        }
    }
}
