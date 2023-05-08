﻿using d_lama_service.Middleware;
using d_lama_service.Models;
using d_lama_service.Models.DataProcessing;
using d_lama_service.Models.ProjectModels;
using d_lama_service.Repositories;
using Data;
using Data.ProjectEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
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
        [HttpGet("{projectId:int}")]
        public async Task<IActionResult> GetAllTextDataPointsAsync(int projectId)
        {
            var project = await GetProjectAsync(projectId);

            var dataPoints = await _unitOfWork.DataPointRepository.FindAsync(e => e.ProjectId == projectId);
            if (!dataPoints.Any())
            {
                return NotFound("No data points found for this project.");
            }
            return Ok(dataPoints);
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

            // TODO: maybe define count in repository and not get whole list here
            var dataPoints = await _unitOfWork.DataPointRepository.FindAsync(e => e.ProjectId == projectId);
            return Ok(dataPoints.Count());
        }

        /// <summary>
        /// Retrieves a data point related to a project by its index.
        /// </summary>
        /// <param name="projectId"> The ID of the project. </param>
        /// <param name="dataPointIndex"> The index of the data point. </param>
        /// <returns> 200 with a list of data points or 404 if there are no data points at all. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet("{projectId:int}/{dataPointIndex:int}")]
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
        [HttpGet("{projectId:int}/{startIndex:int}/{endIndex:int}")]
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
        [HttpPost("{projectId:int}/CreateSingleTextDataPoint")]
        public async Task<IActionResult> CreateSingleTextDataPointAsync(int projectId, [FromBody] TextDataPointModel dataPointForm)
        {
            // Check if the project exists
            var project = await GetProjectWithOwnerCheckAsync(projectId);

            var index = await GetNextTextDataPointIndexAsync(project);
            var dataPoint = new TextDataPoint(dataPointForm.Content, index);
            project.DataPoints.Add(dataPoint);
            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

            var uri = nameof(GetTextDataPointAsync) + "/" + dataPoint.ProjectId + "/" + dataPoint.DataPointIndex;
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
            var project = await GetProjectWithOwnerCheckAsync(projectId);

            // Check if a uploadedFile was uploaded
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            // TODO: check malware with library - not yet - first discuss which tool to use

            // check if uploadedFile in supported format
            DataSetReader dataSetReader = new DataSetReader();

            // read data into database
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
            var project = await GetProjectWithOwnerCheckAsync(projectId);

            // Check if a uploadedFile was uploaded
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            // TODO: check malware with library - not yet - first discuss which tool to use

            string projectPath = ""; // TODO: add property project path to Project and create local path when project is created
            DataSetReader dataSetReader = new DataSetReader();

            // read data into database
            int index = await GetNextImageDataPointIndexAsync(project);
            ICollection<string> imagePaths = await dataSetReader.ReadFileAsync(uploadedFile, index, projectPath);
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
        [HttpDelete("{projectId:int}/DeleteTextDataPoints")]
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
        [HttpDelete("{projectId:int}/DeleteTextDataPoints/{startIndex:int}/{endIndex:int}")]
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

            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveAsync();

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
        public async Task<IActionResult> LabelDataPoint(int projectId, int dataPointIndex, [FromBody] int labelId) 
        {
            var labeledDataPoint = await GetLabeledDataPointOfUserAsync(projectId, dataPointIndex);
            if (labeledDataPoint != null) 
            {
                return BadRequest("Data point is already labeled by you.");
            }

            var user = await GetAuthenticatedUserAsync();
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
            var project = await GetProjectWithOwnerCheckAsync(projectId, e => e.DataPoints);
            var labeledDataModelList = new Dictionary<int,LabeledDataPointModel>();  
            foreach (var dataPoint in project.DataPoints) 
            {
                var dataPointStats = await _unitOfWork.LabeledDataPointRepository.GetLabeledDataPointStatisticAsync(dataPoint.Id);
                labeledDataModelList.Add(dataPoint.Id, new LabeledDataPointModel(dataPointStats));
            }
            return Ok(labeledDataModelList);
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
            await GetProjectWithOwnerCheckAsync(projectId); // for owner check
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
            var user = await GetAuthenticatedUserAsync();
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
        /// <param name="path"> Data point content. </param>
        /// <param name="index"> Data point index. </param>
        /// <returns> The created text data point. </returns>
        private ImageDataPoint CreateImageDataPoint(string path, int index)
        {
            var dataPoint = new ImageDataPoint(path, index);
            _unitOfWork.ImageDataPointRepository.Update(dataPoint);
            return dataPoint;
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

        private async Task<DataPoint> GetDataPointFromProjectAsync(int projectId, int dataPointId) 
        {
            DataPoint? dataPoint = (await _unitOfWork.DataPointRepository.FindAsync(e => e.DataPointIndex == dataPointId && e.ProjectId == projectId)).FirstOrDefault();
            if (dataPoint == null)
            {
                throw new RESTException(HttpStatusCode.NotFound, $"DataPoint with id {dataPointId} does not exist.");
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

        // TODO: refactor, find solution for GetProjectWithOwnerCheckAsync that is a duplicate of the one in ProjectController
        /// <summary>
        /// Gets a project with checking if the user is owner of the project. 
        /// </summary>
        /// <param name="projectId"> The id of the project. </param>
        /// <param name="includes"> The other tables to load (join). </param>
        /// <returns> The found project with its dependencies. </returns>
        /// <exception cref="RESTException"> Throws Rest Excetption if project is not found or the current user is not the owner. </exception>
        private async Task<Project> GetProjectWithOwnerCheckAsync(int projectId, params Expression<Func<Project, object>>[] includes)
        {
            Project project = await GetProjectAsync(projectId, includes);

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
