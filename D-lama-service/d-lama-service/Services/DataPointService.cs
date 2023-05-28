using d_lama_service.Models;
using d_lama_service.Models.DataPointModels;
using d_lama_service.Models.DataProcessing;
using d_lama_service.Models.ProjectModels;
using d_lama_service.Models.UserModels;
using d_lama_service.Repositories.DataPointRepositories;
using d_lama_service.Repositories.ProjectRepositories;
using d_lama_service.Repositories.UserRepositories;
using Data;
using Data.ProjectEntities;
using System.Linq.Expressions;
using System.Net;

namespace d_lama_service.Services
{
    /// <summary>
    /// Implementation of the IDataPointService interface.
    /// The data point service handles the domain specific logic regarding the data points.
    /// </summary>
    public class DataPointService : Service, IDataPointService
    {
        private readonly ITextDataPointRepository _textDataPointRepository;
        private readonly IImageDataPointRepository _imageDataPointRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IDataPointRepository _dataPointRepository;
        private readonly ILabeledDataPointRepository _labeledDataPointRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILabelRepository _labelRepository;

        /// <summary>
        /// Constructor of the DataPointService.
        /// </summary>
        /// <param name="context"> The database context. </param>
        public DataPointService(DataContext context) : base(context)
        {
            _textDataPointRepository = new TextDataPointRepository(context);
            _imageDataPointRepository = new ImageDataPointRepository(context);
            _projectRepository = new ProjectRepository(context);
            _dataPointRepository = new DataPointRepository(context);
            _labeledDataPointRepository = new LabeledDataPointRepository(context);
            _userRepository = new UserRepository(context);
            _labelRepository = new LabelRepository(context);
        }

        public async Task<List<ReadDataPointModel>> GetDataPointsFromProjectAsync(int projectId, User user, int? startIndex = null, int? endIndex = null)
        {
            IEnumerable<DataPoint> dataPoints;
            var project = await GetProjectAsync(projectId);

            if (project.DataType == ProjectDataType.Text)
            {
                dataPoints = await _textDataPointRepository.FindAsync(e => e.ProjectId == projectId);
            }
            else
            {
                dataPoints = await _imageDataPointRepository.FindAsync(e => e.ProjectId == projectId);
            }

            if (startIndex != null && endIndex != null)
            {
                dataPoints = dataPoints.Where(e => e.DataPointIndex >= startIndex && e.DataPointIndex <= endIndex);
            }
            return await CreateReadDataPointModelListAsync(user, project, dataPoints);
        }

        public async Task<int> GetDataPointsCountOfProjectAsync(int projectId)
        {
            var project = await GetProjectAsync(projectId);
            var dataPoints = await _dataPointRepository.FindAsync(e => e.ProjectId == projectId);
            return dataPoints.Count();
        }

        public async Task<ProjectDataType> GetProjectTypeAsync(int projectId)
        {
            var project = await GetProjectAsync(projectId);
            return project.DataType;
        }

        public async Task<ReadTextDataPointModel> GetTextDataPointAsync(User user, int projectId, int dataPointIndex)
        {
            var textDataPoints = await _textDataPointRepository.FindAsync(e => e.ProjectId == projectId && e.DataPointIndex == dataPointIndex);
            if (!textDataPoints.Any())
            {
                throw new RESTException(HttpStatusCode.NotFound, "No data point found for this project and index.");
            }
            var textDataPoint = textDataPoints.First();
            var isLableled = await IsDataPointLabeledByUser(user, projectId, textDataPoint.DataPointIndex);
            return new ReadTextDataPointModel(textDataPoint, isLableled);
        }

        public async Task<(byte[], string)> GetImageDataPointAsync(int projectId, int dataPointIndex)
        {
            var imageDataPoints = await _imageDataPointRepository.FindAsync(e => e.ProjectId == projectId && e.DataPointIndex == dataPointIndex);
            if (!imageDataPoints.Any())
            {
                throw new RESTException(HttpStatusCode.NotFound, "No data point found for this project and index.");
            }
            var imagePath = imageDataPoints.First().Path;

            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
            string contentType = GetContentType(imagePath);
            return (imageBytes, contentType);
        }

        public async Task<int> CreateTextDataPointAsync(Project project, TextDataPointModel dataPointRequest)
        {
            var index = await GetNextTextDataPointIndexAsync(project);
            var dataPoint = new TextDataPoint(dataPointRequest.Content, index);
            project.DataPoints.Add(dataPoint);
            _projectRepository.Update(project);
            await SaveAsync();

            return dataPoint.DataPointIndex;
        }

        public async Task CreateTextDataPointsAsync(Project project, IFormFile file)
        {
            ValidateFile(file, ProjectDataType.Text, project.DataType);

            // read data into database
            DataSetReader dataSetReader = new DataSetReader();
            ICollection<string> textDataPoints = await dataSetReader.ReadFileAsync(file);

            var index = await GetNextTextDataPointIndexAsync(project);
            foreach (var textDataPoint in textDataPoints)
            {
                project.DataPoints.Add(CreateTextDataPoint(textDataPoint, index));
                index++;
            }
            _projectRepository.Update(project);
            await SaveAsync();
        }

        public async Task<int> CreateImageDataPointAsync(Project project, IFormFile image)
        {
            ValidateFile(image, ProjectDataType.Image, project.DataType);

            // read data into database
            DataSetReader dataSetReader = new DataSetReader();
            int index = await GetNextImageDataPointIndexAsync(project);
            ICollection<string> imagePaths = await dataSetReader.ReadFileAsync(image, index, project.StoragePath);
            ImageDataPoint imageDataPoint = CreateImageDataPoint(imagePaths.First(), index);
            project.DataPoints.Add(imageDataPoint);
            _projectRepository.Update(project);
            await SaveAsync();

            return imageDataPoint.DataPointIndex;
        }

        public async Task CreateImageDataPointsAsync(Project project, IFormFile file)
        {
            ValidateFile(file, ProjectDataType.Image, project.DataType);

            // read data into database
            DataSetReader dataSetReader = new DataSetReader();
            int index = await GetNextImageDataPointIndexAsync(project);
            ICollection<string> imagePaths = await dataSetReader.ReadFileAsync(file, index, project.StoragePath);
            foreach (var imagePath in imagePaths)
            {
                // create the image data point and add it to the project
                project.DataPoints.Add(CreateImageDataPoint(imagePath, index));
                index++;
            }

            // save the changes to the database
            _projectRepository.Update(project);
            await SaveAsync();
        }

        public async Task UpdateTextDataPointAsync(Project project, int dataPointIndex, EditTextDataPointModel modifiedTextDataPoint)
        {
            TextDataPoint textDataPoint = (TextDataPoint)await GetDataPointAsync(project, ProjectDataType.Text, dataPointIndex);

            textDataPoint.Content = modifiedTextDataPoint.Content ?? textDataPoint.Content;
            textDataPoint.UpdateDate = DateTime.UtcNow;
            textDataPoint.Version++;
            _textDataPointRepository.Update(textDataPoint);
            await SaveAsync();
        }

        public async Task UpdateImageDataPointAsync(Project project, int dataPointIndex, IFormFile file)
        {
            ImageDataPoint imageDataPoint = (ImageDataPoint)await GetDataPointAsync(project, ProjectDataType.Image, dataPointIndex);

            // replace image in file system
            File.Delete(imageDataPoint.Path);
            DataSetReader dataSetReader = new DataSetReader();
            ICollection<string> imagePaths = await dataSetReader.ReadFileAsync(file, imageDataPoint.DataPointIndex, project.StoragePath);

            imageDataPoint.Path = imagePaths.First();
            imageDataPoint.UpdateDate = DateTime.UtcNow;
            imageDataPoint.Version++;

            _imageDataPointRepository.Update(imageDataPoint);
            await SaveAsync();
        }

        public async Task DeleteDataPointsAsync(Project project, int? startIndex = null, int? endIndex = null)  // TODO Check if really deleted!
        {
            bool isImageProject = project.DataType == ProjectDataType.Image;
            var dataPoints = await _dataPointRepository.FindAsync(e => e.ProjectId == project.Id);
            if (startIndex != null && endIndex != null)
            {
                dataPoints.Where(e => e.DataPointIndex >= startIndex && e.DataPointIndex <= endIndex);
            }

            foreach (var dataPoint in dataPoints)
            {
                if (isImageProject)
                {
                    await DeleteImageDataPointAsync(dataPoint);
                }
                _dataPointRepository.Delete(dataPoint);
            }
            _projectRepository.Update(project);
            await SaveAsync();
        }

        public async Task LabelDataPointsAsync(User user, int projectId, int dataPointIndex, int labelId)
        {
            var labeledDataPoint = await GetLabeledDataPointOfUserAsync(user, projectId, dataPointIndex);
            if (labeledDataPoint != null)
            {
                throw new RESTException(HttpStatusCode.BadRequest, "Data point is already labeled by you.");
            }

            Project project = await GetProjectAsync(projectId); // check for existance
            var label = await GetLabelAsync(labelId);
            var dataPoint = await GetDataPointAsync(project, project.DataType, dataPointIndex);

            if (label.ProjectId != projectId)
            {
                throw new RESTException(HttpStatusCode.BadRequest, "The labels provided do not match with the Project!");
            }

            var labeledData = new LabeledDataPoint { LabelId = labelId, UserId = user.Id, DataPointId = dataPoint.Id };
            _labeledDataPointRepository.Update(labeledData);
            await SaveAsync();
        }

        public async Task DeleteLabeledDataPointAsync(User user, int projectId, int dataPointIndex)
        {
            var labeledDataPoint = await GetLabeledDataPointOfUserAsync(user, projectId, dataPointIndex);
            if (labeledDataPoint == null)
            {
                throw new RESTException(HttpStatusCode.BadRequest, "The labled data point must be first labeled by yourself, before you can delete it.");
            }
            _labeledDataPointRepository.Delete(labeledDataPoint);
            await SaveAsync();
        }

        public async Task<LabeledDataPointModel> GetLabeledDataPointForProject(Project project, int dataPointIndex)
        {
            var dataPointId = (await GetDataPointAsync(project, project.DataType, dataPointIndex)).Id;
            var dataPointStats = await _labeledDataPointRepository.GetLabeledDataPointStatisticAsync(dataPointId);
            return new LabeledDataPointModel(dataPointStats);
        }

        public async Task<LabeledDataModel> GetLabeledDataForProjectAsync(Project project)
        {
            var labeledDataModelDic = new Dictionary<int, LabeledDataPointModel>();
            var userIds = new HashSet<int>();
            foreach (var dataPoint in project.DataPoints)
            {
                var dataPointStats = await _labeledDataPointRepository.GetLabeledDataPointStatisticAsync(dataPoint.Id);
                labeledDataModelDic.Add(dataPoint.DataPointIndex, new LabeledDataPointModel(dataPointStats));
            }
            var projectModel = new ProjectModel(project);

            var userCompletion = await _labeledDataPointRepository.GetLabeledDataPointsProcessOfUser(project.DataPoints.Select(e => e.Id));
            var labellers = new List<LabellerModel>();
            foreach (var user in userCompletion)
            {
                var userInfo = (await _userRepository.GetAsync(user.Key));
                labellers.Add(new LabellerModel { Id = userInfo!.Id, FirstName = userInfo.FirstName, LastName = userInfo.LastName, Percentage = user.Value });
            }

            var labeledData = new LabeledDataModel { DataPoints = labeledDataModelDic, Project = projectModel, Labellers = labellers };
            return labeledData;
        }

        public async Task<List<ReadTextDataPointModel>> GetTextDataPointRangeAsync(int projectId, User user, int? startIndex = null, int? endIndex = null) 
        {
            var project = await GetProjectAsync(projectId);
            var dataPoints = await _textDataPointRepository.FindAsync(e => e.ProjectId == projectId);

            if (startIndex != null && endIndex != null)
            {
                dataPoints = dataPoints.Where(e => e.DataPointIndex >= startIndex && e.DataPointIndex <= endIndex);
            }

            var responseList = new List<ReadTextDataPointModel>();
            foreach (var dataPoint in dataPoints)
            {
                var isLableled = await IsDataPointLabeledByUser(user, project.Id, dataPoint.DataPointIndex);
                responseList.Add(new ReadTextDataPointModel(dataPoint, isLableled));
            }
            return responseList;
        }

        public async Task<List<ReadImageDataPointModel>> GetImageDataPointRangeAsync(int projectId, User user, int? startIndex = null, int? endIndex = null)
        {
            var project = await GetProjectAsync(projectId);
            var dataPoints = await _imageDataPointRepository.FindAsync(e => e.ProjectId == projectId);

            if (startIndex != null && endIndex != null)
            {
                dataPoints = dataPoints.Where(e => e.DataPointIndex >= startIndex && e.DataPointIndex <= endIndex);
            }

            var responseList = new List<ReadImageDataPointModel>();
            foreach (var dataPoint in dataPoints)
            {
                var isLableled = await IsDataPointLabeledByUser(user, project.Id, dataPoint.DataPointIndex);
                responseList.Add(new ReadImageDataPointModel(dataPoint, isLableled));
            }
            return responseList;
        }


        private async Task<Label> GetLabelAsync(int labelId)
        {
            Label? label = await _labelRepository.GetAsync(labelId);
            if (label == null)
            {
                throw new RESTException(HttpStatusCode.NotFound, $"Label with id {labelId} does not exist.");
            }
            return label;
        }

        private async Task DeleteImageDataPointAsync(DataPoint dataPoint)
        {
            ImageDataPoint imageDataPoint = (await _imageDataPointRepository.GetAsync(dataPoint.Id))!;
            try
            {
                File.Delete(imageDataPoint.Path);
            }
            catch (FileNotFoundException e)
            {
                var a = ($"The file was not found: {e.FileName}"); // TODO: LOG instead of giving back
            }
            catch (DirectoryNotFoundException)
            {
                var a = ($"The project directory was not found: project_{imageDataPoint.ProjectId}"); // TODO: LOG instead of giving back
            }
            catch (IOException)
            {
                var a = ($"The file could not be deleted because it is currently in use: {Path.GetFileName(imageDataPoint.Path)}"); // TODO: LOG instead of giving back
            }
        }

        private async Task<DataPoint> GetDataPointAsync(Project project, ProjectDataType wantedType, int dataPointIndex)
        {
            ValidateProjectType(wantedType, project.DataType);
            DataPoint? dataPoint;
            if (project.DataType == ProjectDataType.Text)
            {
                dataPoint = (await _textDataPointRepository.FindAsync(e => e.ProjectId == project.Id && e.DataPointIndex == dataPointIndex)).FirstOrDefault();
            }
            else
            {
                dataPoint = (await _imageDataPointRepository.FindAsync(e => e.ProjectId == project.Id && e.DataPointIndex == dataPointIndex)).FirstOrDefault();
            }
            if (dataPoint == null)
            {
                throw new RESTException(HttpStatusCode.NotFound, "Data point not found.");
            }
            return dataPoint;
        }

        private void ValidateFile(IFormFile file, ProjectDataType wantedType, ProjectDataType realType)
        {

            ValidateProjectType(wantedType, realType);
            if (file == null || file.Length == 0) // Check if a uploadedFile was uploaded
            {
                throw new RESTException(HttpStatusCode.BadRequest, "No file was uploaded.");
            }
        }

        private void ValidateProjectType(ProjectDataType wantedType, ProjectDataType realType)
        {
            if (wantedType != realType)
            {
                string type = wantedType == ProjectDataType.Text ? "text" : "image";
                throw new RESTException(HttpStatusCode.BadRequest, $"The project data type must be set to {type} in order that this request can proceed.");
            }
        }

        /// <summary>
        /// Returns the next DataPointIndex for an ImageDataPoint. 
        /// </summary>
        /// <param name="project"> The project. </param>
        /// <returns> Next index. </returns>
        private async Task<int> GetNextImageDataPointIndexAsync(Project project)
        {
            var presentDataPoints = await _imageDataPointRepository.FindAsync(e => e.ProjectId == project.Id);
            int index = presentDataPoints.OrderByDescending(dp => dp.DataPointIndex).FirstOrDefault()?.DataPointIndex + 1 ?? 0;
            return index;
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
            _imageDataPointRepository.Update(dataPoint);
            return dataPoint;
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
            _textDataPointRepository.Update(dataPoint);
            return dataPoint;
        }

        /// <summary>
        /// Returns the next DataPointIndex for a TextDataPoint. 
        /// </summary>
        /// <param name="project"> The project. </param>
        /// <returns> Next index. </returns>
        private async Task<int> GetNextTextDataPointIndexAsync(Project project)
        {
            var presentDataPoints = await _textDataPointRepository.FindAsync(e => e.ProjectId == project.Id);
            int index = presentDataPoints.OrderByDescending(dp => dp.DataPointIndex).FirstOrDefault()?.DataPointIndex + 1 ?? 0;
            return index;
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

        private async Task<List<ReadDataPointModel>> CreateReadDataPointModelListAsync(User user, Project project, IEnumerable<DataPoint> dataPoints)
        {
            var responseList = new List<ReadDataPointModel>();
            foreach (var dataPoint in dataPoints)
            {
                var isLableled = await IsDataPointLabeledByUser(user, project.Id, dataPoint.DataPointIndex);
                if (project.DataType == ProjectDataType.Text)
                {
                    responseList.Add(new ReadTextDataPointModel((TextDataPoint)dataPoint, isLableled));
                }
                else
                {
                    responseList.Add(new ReadImageDataPointModel((ImageDataPoint)dataPoint, isLableled));
                }
            }
            return responseList;
        }

        private async Task<bool> IsDataPointLabeledByUser(User user, int projectId, int dataPointIndex)
        {
            var labeledDataPoint = await GetLabeledDataPointOfUserAsync(user, projectId, dataPointIndex);
            return labeledDataPoint != null;
        }

        /// <summary>
        /// Gets a labeled datapoint of a project with checking gor the user.
        /// </summary>
        /// <param name="projectId"> The project id where the datapoint is belonging to. </param>
        /// <param name="dataPointIndex"> The data point index to be deleted. </param>
        /// <returns> The LabeledDataPoint if found or null if not found. </returns>
        private async Task<LabeledDataPoint?> GetLabeledDataPointOfUserAsync(User user, int projectId, int dataPointIndex)
        {
            var project = await GetProjectAsync(projectId); // check for existance
            var dataPointId = (await GetDataPointAsync(project, project.DataType, dataPointIndex)).Id;
            try
            {
                var labeledDataPoint = (await _labeledDataPointRepository.FindAsync(e => e.UserId == user.Id && e.DataPointId == dataPointId))?.First() ?? null;
                return labeledDataPoint;
            }
            catch
            {
                return null;
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
            Project? project = await _projectRepository.GetDetailsAsync(projectId, includes);
            if (project == null)
            {
                throw new RESTException(HttpStatusCode.NotFound, $"Project with id {projectId} does not exist.");
            }
            return project;
        }
    }
}
