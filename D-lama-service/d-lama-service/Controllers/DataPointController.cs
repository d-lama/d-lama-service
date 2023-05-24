using d_lama_service.Middleware;
using d_lama_service.Models;
using d_lama_service.Models.DataPointModels;
using d_lama_service.Models.DataProcessing;
using d_lama_service.Models.ProjectModels;
using d_lama_service.Models.UserModels;
using d_lama_service.Repositories;
using d_lama_service.Services;
using Data.ProjectEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Net;
using System.Text;

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
        private readonly IDataPointService _dataPointService;
        private readonly ISharedService _sharedService;

        /// <summary>
        /// Constructor of the DataPointController.
        /// </summary>
        public DataPointController(IUnitOfWork unitOfWork, IDataPointService dataPointService, ISharedService sharedService)
        {
            _unitOfWork = unitOfWork;
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
        public async Task<IActionResult> GetAllDataPointsAsync(int projectId)
        {
            var project = await GetProjectAsync(projectId);

            if (project.DataType == ProjectDataType.Text)
            {
                var responseList = new List<ReadTextDataPointModel>();
                var textDataPoints = await _unitOfWork.TextDataPointRepository.FindAsync(e => e.ProjectId == projectId);
                if (!textDataPoints.Any())
                {
                    return NotFound("No data point found for this project.");
                }

                foreach (var textDataPoint in textDataPoints)
                {
                    var isLableled = await IsDataPointLabeledByUser(project.Id, textDataPoint.DataPointIndex);
                    responseList.Add(new ReadTextDataPointModel(textDataPoint, isLableled));
                }

                return Ok(responseList);
            }
            else if (project.DataType == ProjectDataType.Image)
            {
                var responseList = new List<ReadImageDataPointModel>();
                var imageDataPoints = await _unitOfWork.ImageDataPointRepository.FindAsync(e => e.ProjectId == projectId);
                if (!imageDataPoints.Any())
                {
                    return NotFound("No data point found for this project.");
                }
                
                foreach (var imageDataPoint in imageDataPoints)
                {
                    var isLableled = await IsDataPointLabeledByUser(project.Id, imageDataPoint.DataPointIndex);
                    responseList.Add(new ReadImageDataPointModel(imageDataPoint, isLableled));
                }

                return Ok(responseList);
            }
            else
            {
                return NotFound("No data point found for this project.");
            }
        }

        /// <summary>
        /// Retrieves a the number of data points related to a project.
        /// </summary>
        /// <param name="projectId"> The ID of the project. </param>
        /// <returns> The number of text data points. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet("{projectId:int}/GetNumberOfDataPoints")]
        public async Task<IActionResult> GetNumberOfDataPointsAsync(int projectId)
        {
            var project = await GetProjectAsync(projectId);

            var dataPoints = await _unitOfWork.DataPointRepository.FindAsync(e => e.ProjectId == projectId);
            return Ok(dataPoints.Count());
        }

        /// <summary>
        /// Retrieves a data point related to a project by its index.
        /// </summary>
        /// <param name="projectId"> The ID of the project. </param>
        /// <param name="dataPointIndex"> The index of the data point. </param>
        /// <returns> 200 with the data point or 404 if not match found. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet("{projectId:int}/{dataPointIndex:int}")]
        public async Task<IActionResult> GetDataPointAsync(int projectId, int dataPointIndex)
        {
            var project = await GetProjectAsync(projectId);

            if (project.DataType == ProjectDataType.Text)
            {
                var textDataPoints = await _unitOfWork.TextDataPointRepository.FindAsync(e => e.ProjectId == projectId && e.DataPointIndex == dataPointIndex);
                if (!textDataPoints.Any())
                {
                    return NotFound("No data point found for this project and index.");
                }
                var textDataPoint = textDataPoints.First();
                var isLableled = await IsDataPointLabeledByUser(project.Id, textDataPoint.DataPointIndex);
                return Ok(new ReadTextDataPointModel(textDataPoint, isLableled));
            }
            else if (project.DataType == ProjectDataType.Image)
            {
                var imageDataPoints = await _unitOfWork.ImageDataPointRepository.FindAsync(e => e.ProjectId == projectId && e.DataPointIndex == dataPointIndex);
                if (!imageDataPoints.Any())
                {
                    return NotFound("No data point found for this project and index.");
                }
                var imagePath = imageDataPoints.First().Path;

                byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
                string contentType = GetContentType(imagePath);

                return File(imageBytes, contentType);
            }
            else
            {
                return NotFound("No data point found for this project and index.");
            }            
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
        public async Task<IActionResult> GetDataPointRangeAsync(int projectId, int startIndex, int endIndex)
        {
            var project = await GetProjectAsync(projectId);

            if (project.DataType == ProjectDataType.Text)
            {
                var responseList = new List<ReadTextDataPointModel>();
                var textDataPoints = await _unitOfWork.TextDataPointRepository
                    .FindAsync(e => e.ProjectId == projectId && e.DataPointIndex >= startIndex && e.DataPointIndex <= endIndex);

                if (!textDataPoints.Any())
                {
                    return NotFound("No data point found for this project.");
                }

                foreach (var textDataPoint in textDataPoints)
                {
                    var isLableled = await IsDataPointLabeledByUser(project.Id, textDataPoint.DataPointIndex);
                    responseList.Add(new ReadTextDataPointModel(textDataPoint, isLableled));
                }

                return Ok(responseList);
            }
            else if (project.DataType == ProjectDataType.Image)
            {
                var responseList = new List<ReadImageDataPointModel>();
                var imageDataPoints = await _unitOfWork.ImageDataPointRepository
                    .FindAsync(e => e.ProjectId == projectId && e.DataPointIndex >= startIndex && e.DataPointIndex <= endIndex);

                if (!imageDataPoints.Any())
                {
                    return NotFound("No data points found for this project.");
                }

                foreach (var imageDataPoint in imageDataPoints)
                {
                    var isLableled = await IsDataPointLabeledByUser(project.Id, imageDataPoint.DataPointIndex);
                    responseList.Add(new ReadImageDataPointModel(imageDataPoint, isLableled));
                }

                return Ok(responseList);
            }
            else
            {
                return NotFound("No data point found for this project.");
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
        public async Task<IActionResult> CreateSingleTextDataPointAsync(int projectId, [FromBody] TextDataPointModel dataPointForm)
        {
            // Check if the project exists
            var project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);

            var index = await GetNextTextDataPointIndexAsync(project);
            var dataPoint = new TextDataPoint(dataPointForm.Content, index);
            project.DataPoints.Add(dataPoint);
            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

            var uri = nameof(GetDataPointAsync) + "/" + dataPoint.ProjectId + "/" + dataPoint.DataPointIndex;
            return Created(uri, dataPoint.Content);
        }

        /// <summary>
        /// Uploads a textual dataset and assigns the content to a given project.
        /// </summary>
        /// <param name="projectId"> The project ID. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [AdminAuthorize]
        [HttpPost("{projectId:int}/UploadTextDataPoints")]
        public async Task<IActionResult> UploadTextDataPointsAsync(int projectId, IFormFile uploadedFile)
        {
            // Check if the project exists
            var project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);

            if (project.DataType != ProjectDataType.Text)
            {
                return BadRequest("The project data type must be set to text in order to upload text content.");
            }

            // Check if a uploadedFile was uploaded
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            // read data into database
            DataSetReader dataSetReader = new DataSetReader();
            ICollection<string> textDataPoints = await dataSetReader.ReadFileAsync(uploadedFile);

            var index = await GetNextTextDataPointIndexAsync(project);
            foreach (var textDataPoint in textDataPoints)
            {
                project.DataPoints.Add(CreateTextDataPoint(textDataPoint, index));
                index++;
            }

            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

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
        public async Task<IActionResult> UploadSingleImageDataPointAsync(int projectId, IFormFile uploadedFile)
        {
            // Check if the project exists
            var project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);

            if (project.DataType != ProjectDataType.Image)
            {
                return BadRequest("The project data type must be set to image in order to upload image files.");
            }

            // Check if a uploadedFile was uploaded
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            // read data into database
            DataSetReader dataSetReader = new DataSetReader();
            int index = await GetNextImageDataPointIndexAsync(project);
            ICollection<string> imagePaths = await dataSetReader.ReadFileAsync(uploadedFile, index, project.StoragePath);
            project.DataPoints.Add(CreateImageDataPoint(imagePaths.First(), index));

            // save the changes to the database
            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

            return Ok();
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
        public async Task<IActionResult> UploadImageDataPointsAsync(int projectId, IFormFile uploadedFile)
        {
            // Check if the project exists
            var project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);

            if (project.DataType != ProjectDataType.Image)
            {
                return BadRequest("The project data type must be set to image in order to upload image files.");
            }

            // Check if a uploadedFile was uploaded
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            // read data into database
            DataSetReader dataSetReader = new DataSetReader();
            int index = await GetNextImageDataPointIndexAsync(project);
            ICollection<string> imagePaths = await dataSetReader.ReadFileAsync(uploadedFile, index, project.StoragePath);
            foreach (var imagePath in imagePaths)
            {
                // create the image data point and add it to the project
                project.DataPoints.Add(CreateImageDataPoint(imagePath, index));
                index++;
            }

            // save the changes to the database
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
        [HttpPatch("{projectId:int}/EditTextDataPoint/{dataPointIndex:int}")]
        public async Task<IActionResult> EditTextDataPointAsync(int projectId, int dataPointIndex, [FromBody] EditTextDataPointModel dataPointForm)
        {
            // Check if the project exists and if user is owner
            var project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);

            if (project.DataType != ProjectDataType.Text)
            {
                return BadRequest($"The project {project.Name} with ID {project.Id} is not suitable for text edits.");
            }

            var textDataPoints = await _unitOfWork.TextDataPointRepository
               .FindAsync(e => e.ProjectId == projectId && e.DataPointIndex == dataPointIndex);

            if (!textDataPoints.Any())
            {
                return NotFound("Data point not found.");
            }

            var textDataPoint = textDataPoints.First();

            textDataPoint.Content = dataPointForm.Content ?? textDataPoint.Content;
            textDataPoint.UpdateDate = DateTime.UtcNow;
            textDataPoint.Version++;

            _unitOfWork.TextDataPointRepository.Update(textDataPoint);
            await _unitOfWork.SaveAsync();

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
        public async Task<IActionResult> EditImageDataPointAsync(int projectId, int dataPointIndex, IFormFile uploadedFile)
        {
            // Check if the project exists and if user is owner
            var project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);

            if (project.DataType != ProjectDataType.Image)
            {
                return BadRequest($"The project {project.Name} with ID {project.Id} is not suitable for image edits.");
            }

            var imageDataPoints = await _unitOfWork.ImageDataPointRepository
               .FindAsync(e => e.ProjectId == projectId && e.DataPointIndex == dataPointIndex);

            if (!imageDataPoints.Any())
            {
                return NotFound("Data point not found.");
            }

            var imageDataPoint = imageDataPoints.First();

            // replace image in file system
            System.IO.File.Delete(imageDataPoint.Path);
            DataSetReader dataSetReader = new DataSetReader();
            ICollection<string> imagePaths = await dataSetReader.ReadFileAsync(uploadedFile, imageDataPoint.DataPointIndex, project.StoragePath);

            imageDataPoint.Path = imagePaths.First();
            imageDataPoint.UpdateDate = DateTime.UtcNow;
            imageDataPoint.Version++;

            _unitOfWork.ImageDataPointRepository.Update(imageDataPoint);
            await _unitOfWork.SaveAsync();

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
        public async Task<IActionResult> DeleteAllDataPointsAsync(int projectId)
        {
            // Check if the project exists and if user is owner
            var project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);

            if (project.DataType == ProjectDataType.Text)
            {
                var textDataPoints = await _unitOfWork.TextDataPointRepository.FindAsync(e => e.ProjectId == projectId);

                if (!textDataPoints.Any())
                {
                    return NotFound("No data point found for this project.");
                }

                foreach (var textDataPoint in textDataPoints)
                {
                    _unitOfWork.TextDataPointRepository.Delete(textDataPoint);
                }

                _unitOfWork.ProjectRepository.Update(project);
                await _unitOfWork.SaveAsync();

                return Ok();
            }
            else if (project.DataType == ProjectDataType.Image)
            {
                var imageDataPoints = await _unitOfWork.ImageDataPointRepository.FindAsync(e => e.ProjectId == projectId);

                if (!imageDataPoints.Any())
                {
                    return NotFound("No data points found for this project.");
                }

                var response = DeleteImageDataPoints(imageDataPoints);

                _unitOfWork.ProjectRepository.Update(project);
                await _unitOfWork.SaveAsync();

                return Ok(response);
            }
            else
            {
                return NotFound("No data point found for this project.");
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
        public async Task<IActionResult> DeleteTextDataPointRangeAsync(int projectId, int startIndex, int endIndex)
        {
            // Check if the project exists and if user is owner
            var project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext);

            if (project.DataType == ProjectDataType.Text)
            {
                var textDataPoints = await _unitOfWork.TextDataPointRepository
                    .FindAsync(e => e.ProjectId == projectId && e.DataPointIndex >= startIndex && e.DataPointIndex <= endIndex);

                if (!textDataPoints.Any())
                {
                    return NotFound("No data point found for this project.");
                }

                foreach (var textDataPoint in textDataPoints)
                {
                    _unitOfWork.TextDataPointRepository.Delete(textDataPoint);
                }

                _unitOfWork.ProjectRepository.Update(project);
                await _unitOfWork.SaveAsync();

                return Ok();
            }
            else if (project.DataType == ProjectDataType.Image)
            {
                var imageDataPoints = await _unitOfWork.ImageDataPointRepository
                    .FindAsync(e => e.ProjectId == projectId && e.DataPointIndex >= startIndex && e.DataPointIndex <= endIndex);

                if (!imageDataPoints.Any())
                {
                    return NotFound("No data points found for this project.");
                }

                var response = DeleteImageDataPoints(imageDataPoints);

                _unitOfWork.ProjectRepository.Update(project);
                await _unitOfWork.SaveAsync();

                return Ok(response);
            }
            else
            {
                return NotFound("No data point found for this project.");
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
        public async Task<IActionResult> LabelDataPoint(int projectId, int dataPointIndex, [FromBody] int labelId) 
        {
            var labeledDataPoint = await GetLabeledDataPointOfUserAsync(projectId, dataPointIndex);
            if (labeledDataPoint != null) 
            {
                return BadRequest("Data point is already labeled by you.");
            }

            var user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
            await GetProjectAsync(projectId); // check for existance
            var label = await GetLabelAsync(labelId);
            var dataPoint = await GetDataPointFromProjectAsync(projectId, dataPointIndex);
            
            if (label.ProjectId != projectId) 
            {
                return BadRequest("The labels provided do not match with the Project!");
            }

            var labeledData = new LabeledDataPoint { LabelId = labelId, UserId = user.Id, DataPointId = dataPoint.Id };
            _unitOfWork.LabeledDataPointRepository.Update(labeledData);
            await _unitOfWork.SaveAsync();
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
        public async Task<IActionResult> RemoveLabelDataPoint(int projectId, int dataPointIndex) 
        {
            var labeledDataPoint = await GetLabeledDataPointOfUserAsync(projectId, dataPointIndex);
            if (labeledDataPoint == null)
            {
                return BadRequest("The labled data point must be first labeled by yourself, before you can delete it.");
            }
            _unitOfWork.LabeledDataPointRepository.Delete(labeledDataPoint);
            await _unitOfWork.SaveAsync();

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
        public async Task<IActionResult> GetLabeledDataForProject(int projectId) 
        {
            var project = await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext, e => e.DataPoints, e => e.Labels);
            var labeledDataModelDic = new Dictionary<int,LabeledDataPointModel>();  
            var userIds = new HashSet<int>();
            foreach (var dataPoint in project.DataPoints) 
            {
                var dataPointStats = await _unitOfWork.LabeledDataPointRepository.GetLabeledDataPointStatisticAsync(dataPoint.Id);
                labeledDataModelDic.Add(dataPoint.DataPointIndex, new LabeledDataPointModel(dataPointStats));
            }
            var projectModel = new ProjectModel(project);

            var userCompletion = await _unitOfWork.LabeledDataPointRepository.GetLabeledDataPointsProcessOfUser(project.DataPoints.Select(e => e.Id));
            var labellers = new List<LabellerModel>();
            foreach(var user in userCompletion)
            {
                var userInfo = (await _unitOfWork.UserRepository.GetAsync(user.Key));
                labellers.Add(new LabellerModel { Id = userInfo!.Id, FirstName = userInfo.FirstName, LastName = userInfo.LastName, Percentage = user.Value });
            }

            var viewModel = new LabeledDataModel { DataPoints = labeledDataModelDic, Project = projectModel, Labellers = labellers };
            return Ok(viewModel);
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
        public async Task<IActionResult> GetLabeledDataPointForProject(int projectId, int dataPointIndex) 
        {
            await _sharedService.GetProjectWithOwnerCheckAsync(projectId, HttpContext); // for owner check
            var dataPointId = (await GetDataPointFromProjectAsync(projectId, dataPointIndex)).Id;
            var dataPointStats = await _unitOfWork.LabeledDataPointRepository.GetLabeledDataPointStatisticAsync(dataPointId);
            return Ok(new LabeledDataPointModel(dataPointStats));
        }

        /// <summary>
        /// Gets a labeled datapoint of a project with checking gor the user.
        /// </summary>
        /// <param name="projectId"> The project id where the datapoint is belonging to. </param>
        /// <param name="dataPointIndex"> The data point index to be deleted. </param>
        /// <returns> The LabeledDataPoint if found or null if not found. </returns>
        private async Task<LabeledDataPoint?> GetLabeledDataPointOfUserAsync(int projectId, int dataPointIndex) 
        {
            var user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
            await GetProjectAsync(projectId); // check for existance
            var dataPointId = (await GetDataPointFromProjectAsync(projectId, dataPointIndex)).Id;
            try
            {
                var labeledDataPoint = (await _unitOfWork.LabeledDataPointRepository.FindAsync(e => e.UserId == user.Id && e.DataPointId == dataPointId))?.First() ?? null;
                return labeledDataPoint;
            }
            catch 
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the next DataPointIndex for a TextDataPoint. 
        /// </summary>
        /// <param name="project"> The project. </param>
        /// <returns> Next index. </returns>
        private async Task<int> GetNextTextDataPointIndexAsync(Project project)
        {
            var presentDataPoints = await _unitOfWork.TextDataPointRepository.FindAsync(e => e.ProjectId == project.Id);
            int index = presentDataPoints.OrderByDescending(dp => dp.DataPointIndex).FirstOrDefault()?.DataPointIndex + 1 ?? 0;
            return index;
        }

        /// <summary>
        /// Returns the next DataPointIndex for an ImageDataPoint. 
        /// </summary>
        /// <param name="project"> The project. </param>
        /// <returns> Next index. </returns>
        private async Task<int> GetNextImageDataPointIndexAsync(Project project)
        {
            var presentDataPoints = await _unitOfWork.ImageDataPointRepository.FindAsync(e => e.ProjectId == project.Id);
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
        /// Creates an image data point with given path and index. 
        /// </summary>
        /// <param name="path"> Data point path. </param>
        /// <param name="index"> Data point index. </param>
        /// <returns> The created image data point. </returns>
        private ImageDataPoint CreateImageDataPoint(string path, int index)
        {
            var dataPoint = new ImageDataPoint(path, index);
            _unitOfWork.ImageDataPointRepository.Update(dataPoint);
            return dataPoint;
        }

        private string DeleteImageDataPoint(ImageDataPoint imageDataPoint)
        {
            string responseLogLine = "";
            try
            {
                System.IO.File.Delete(imageDataPoint.Path);
            }
            catch (FileNotFoundException e)
            {
                responseLogLine = ($"The file was not found: {e.FileName}");
            }
            catch (DirectoryNotFoundException)
            {
                responseLogLine = ($"The project directory was not found: project_{imageDataPoint.ProjectId}");
            }
            catch (IOException)
            {
                responseLogLine = ($"The file could not be deleted because it is currently in use: {Path.GetFileName(imageDataPoint.Path)}");
            }
            finally
            {
                _unitOfWork.ImageDataPointRepository.Delete(imageDataPoint);
            }
            return responseLogLine;
        }

        private string DeleteImageDataPoints(IEnumerable<ImageDataPoint> imageDataPoints)
        {
            var responseLog = new StringBuilder();

            foreach (var imageDataPoint in imageDataPoints)
            {
                var responseLogLine = DeleteImageDataPoint(imageDataPoint);
                if (!string.IsNullOrEmpty(responseLogLine))
                {
                    responseLog.AppendLine(responseLogLine);
                }
            }
            return responseLog.ToString();
        }

        /// <summary>
        /// Gets the content type of an image file at the given path.
        /// </summary>
        /// <param name="imagePath"> Data point path. </param>
        /// <returns> The content type. </returns>
        private string GetContentType(string imagePath)
        {
            string extension = Path.GetExtension(imagePath);
            switch (extension.ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                default:
                    return "application/octet-stream";
            }
        }

        /// <summary>
        /// Gets a project with checking if it exists. 
        /// </summary>
        /// <param name="projectId"> The id of the project. </param>
        /// <returns> The found project. </returns>
        /// <exception cref="RESTException"> Throws Rest Excetption if project is not found. </exception>
        private async Task<Project> GetProjectAsync(int projectId, params Expression<Func<Project, object>>[] includes)
        {
            Project? project = await _unitOfWork.ProjectRepository.GetDetailsAsync(projectId,includes);
            if (project == null)
            {
                throw new RESTException(HttpStatusCode.NotFound, $"Project with id {projectId} does not exist.");
            }
            return project;
        }

        private async Task<bool> IsDataPointLabeledByUser(int projectId, int dataPointIndex)
        {
            var labeledDataPoint = await GetLabeledDataPointOfUserAsync(projectId, dataPointIndex);
            if (labeledDataPoint != null)
            {
                return true;
            }
            return false;
        }

        private async Task<DataPoint> GetDataPointFromProjectAsync(int projectId, int dataPointIndex) 
        {
            DataPoint? dataPoint = (await _unitOfWork.DataPointRepository.FindAsync(e => e.DataPointIndex == dataPointIndex && e.ProjectId == projectId)).FirstOrDefault();
            if (dataPoint == null)
            {
                throw new RESTException(HttpStatusCode.NotFound, $"DataPoint with id {dataPointIndex} does not exist.");
            }
            return dataPoint!;
        }

        private async Task<Label> GetLabelAsync(int labelId)
        {
            Label? label = await _unitOfWork.LabelRepository.GetAsync(labelId);
            if (label == null)
            {
                throw new RESTException(HttpStatusCode.NotFound, $"Label with id {labelId} does not exist.");
            }
            return label;
        }
    }
}
