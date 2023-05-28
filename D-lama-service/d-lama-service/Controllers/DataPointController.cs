using d_lama_service.Middleware;
using d_lama_service.Models.DataPointModels;
using d_lama_service.Models.ProjectModels;
using d_lama_service.Services;
using Data;
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
        private readonly IDataPointService _dataPointService;
        private readonly ISharedService _sharedService;

        /// <summary>
        /// Constructor of the DataPointController.
        /// </summary>
        public DataPointController(IDataPointService dataPointService, ISharedService sharedService)
        {
            _dataPointService = dataPointService;
            _sharedService = sharedService;
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
            User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
            List<ReadDataPointModel> dataPoints = await _dataPointService.GetDataPointsFromProjectAsync(projectId,user);
            return Ok(dataPoints);
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
            int count = await _dataPointService.GetDataPointsCountOfProjectAsync(projectId);
            return Ok(count);
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
            ProjectDataType dataType = await _dataPointService.GetProjectTypeAsync(projectId);
            if (dataType == ProjectDataType.Text)
            {
                User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
                ReadTextDataPointModel textDatPoint = await _dataPointService.GetTextDataPointAsync(user, projectId, dataPointIndex);
                return Ok(textDatPoint);
            }
            (byte[] imageBytes ,string contentType) = await _dataPointService.GetImageDataPointAsync(projectId, dataPointIndex);
            return File(imageBytes, contentType);      
        }

        /// <summary>
        /// Retrieves a range of data point related to a project.
        /// </summary>
        /// <param name="projectId"> The ID of the project. </param>
        /// <param name="startIndex"> The start index of DataPoint range (inclusive). </param>
        /// <param name="endIndex"> The end index of DataPoint range (inclusive). </param>
        /// <returns> The data points. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet("{projectId:int}/{startIndex:int}/{endIndex:int}")]
        public async Task<IActionResult> GetRange(int projectId, int startIndex, int endIndex)
        {
            User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
            ProjectDataType dataType = await _dataPointService.GetProjectTypeAsync(projectId);

            if (dataType == ProjectDataType.Text)
            {
                var dataPoints = await _dataPointService.GetTextDataPointRangeAsync(projectId, user, startIndex, endIndex);
                return Ok(dataPoints);
            }
            var imageDataPoints = await _dataPointService.GetImageDataPointRangeAsync(projectId, user, startIndex, endIndex);
            return Ok(imageDataPoints);
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
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
            int dataPointIndex = await _dataPointService.CreateTextDataPointAsync(project, dataPointForm);
            string uri = nameof(Get) + "/" + projectId + "/" + dataPointIndex;
            return Created(uri, dataPointIndex);
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
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
            await _dataPointService.CreateTextDataPointsAsync(project, uploadedFile);
            return Ok();
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
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
            int dataPointIndex = await _dataPointService.CreateImageDataPointAsync(project, uploadedFile);
            string uri = nameof(Get) + "/" + projectId + "/" + dataPointIndex;
            return Created(uri, dataPointIndex);
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
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
            await _dataPointService.CreateImageDataPointsAsync(project, uploadedFile);
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
        [HttpPatch("{projectId:int}/EditTextDataPoint/{dataPointIndex:int}")]
        public async Task<IActionResult> EditTextPoint(int projectId, int dataPointIndex, [FromBody] EditTextDataPointModel dataPointForm)
        {
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
            await _dataPointService.UpdateTextDataPointAsync(project, dataPointIndex, dataPointForm);
            return Ok();
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
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
            await _dataPointService.UpdateImageDataPointAsync(project, dataPointIndex, uploadedFile);
            return Ok();
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
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
            await _dataPointService.DeleteDataPointsAsync(project);
            return Ok();
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
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);
            await _dataPointService.DeleteDataPointsAsync(project, startIndex, endIndex);
            return Ok();
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
            User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
            await _dataPointService.LabelDataPointsAsync(user, projectId, dataPointIndex, labelId);
            return Ok();
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
            User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
            await _dataPointService.DeleteLabeledDataPointAsync(user, projectId, dataPointIndex);
            return Ok();
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
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext, e => e.DataPoints, e => e.Labels);
            LabeledDataModel labeledData = await _dataPointService.GetLabeledDataForProjectAsync(project);
            return Ok(labeledData);
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
            Project project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext); // for owner check
            LabeledDataPointModel labeledDataPoint = await _dataPointService.GetLabeledDataPointForProject(project, dataPointIndex);
            return Ok(labeledDataPoint);
        }
    }
}
