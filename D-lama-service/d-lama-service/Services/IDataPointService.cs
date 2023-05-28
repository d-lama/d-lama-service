using d_lama_service.Models.DataPointModels;
using d_lama_service.Models.ProjectModels;
using Data;
using Data.ProjectEntities;

namespace d_lama_service.Services
{
    /// <summary>
    /// Interface of the DataPointService.
    /// </summary>
    public interface IDataPointService
    {
        /// <summary>
        /// Creates an image data point.
        /// </summary>
        /// <param name="project"> The project where the image data point should be assigned. </param>
        /// <param name="image"> The image. </param>
        /// <returns> The id of the created image data point. </returns>
        Task<int> CreateImageDataPointAsync(Project project, IFormFile image);
        
        /// <summary>
        /// Creates multiple image data points. 
        /// </summary>
        /// <param name="project"> The project where the image data points should be assigned to. </param>
        /// <param name="file"> A zip file containing all images in it. </param>
        /// <returns></returns>
        Task CreateImageDataPointsAsync(Project project, IFormFile file);

        /// <summary>
        /// Creates a text data point.
        /// </summary>
        /// <param name="project"> The project where the text data point should be assigned. </param>
        /// <param name="dataPointRequest"> The data point request. </param>
        /// <returns> The id of the created text data point. </returns>
        Task<int> CreateTextDataPointAsync(Project project, TextDataPointModel dataPointRequest);

        /// <summary>
        /// Creates multiple text data points.
        /// </summary>
        /// <param name="project"> The project where the text data points should be assigned to. </param>
        /// <param name="file"> A file containing multiple text data points. </param>
        /// <returns></returns>
        Task CreateTextDataPointsAsync(Project project, IFormFile file);
        
        /// <summary>
        /// Deletes data points.
        /// </summary>
        /// <param name="project"> The project which contains the data point. </param>
        /// <param name="startIndex"> The first data point index which should be deleted. </param>
        /// <param name="endIndex"> The last data point index which should be deleted. </param>
        /// <returns></returns>
        Task DeleteDataPointsAsync(Project project, int? startIndex = null, int? endIndex = null);

        /// <summary>
        /// Delete labeled data ponints.
        /// </summary>
        /// <param name="user"> The user who has labeled the data point. </param>
        /// <param name="projectId"> The project id, where the data points belong to. </param>
        /// <param name="dataPointIndex"> The data point index to delete. </param>
        /// <returns></returns>
        Task DeleteLabeledDataPointAsync(User user, int projectId, int dataPointIndex);

        /// <summary>
        /// Gets the number of datapoints for a project. 
        /// </summary>
        /// <param name="projectId"> The project id. </param>
        /// <returns> The number of datapoints. </returns>
        Task<int> GetDataPointsCountOfProjectAsync(int projectId);

        /// <summary>
        /// Gets data points from a project.
        /// </summary>
        /// <param name="projectId"> The project id, where the data points are assigned to. </param>
        /// <param name="user"> The user making the request. </param>
        /// <param name="startIndex"> The start data point index. </param>
        /// <param name="endIndex"> The end data point index. </param>
        /// <returns> A list containing all data points from start to end index. </returns>
        Task<List<ReadDataPointModel>> GetDataPointsFromProjectAsync(int projectId, User user, int? startIndex = null, int? endIndex = null);
        
        /// <summary>
        /// Gets an image data point.
        /// </summary>
        /// <param name="projectId"> The id of the project, where the data point is assigned to. </param>
        /// <param name="dataPointIndex"> The index of the data point itself. </param>
        /// <returns> The image content as the content type. </returns>
        Task<(byte[], string)> GetImageDataPointAsync(int projectId, int dataPointIndex);
        
        /// <summary>
        /// Gets all the labeled data for a project.
        /// </summary>
        /// <param name="project"> The project. </param>
        /// <returns> The labeled data. </returns>
        Task<LabeledDataModel> GetLabeledDataForProjectAsync(Project project);
        
        /// <summary>
        /// Gets one labeled data point of a project. 
        /// </summary>
        /// <param name="project"> The project. </param>
        /// <param name="dataPointIndex"> The index of the data point. </param>
        /// <returns> The labeled data point. </returns>
        Task<LabeledDataPointModel> GetLabeledDataPointForProject(Project project, int dataPointIndex);
        
        /// <summary>
        /// Gets the project type.
        /// </summary>
        /// <param name="projectId"> The id of the project. </param>
        /// <returns> The project type. </returns>
        Task<ProjectDataType> GetProjectTypeAsync(int projectId);
        
        /// <summary>
        /// Gets a text data point. 
        /// </summary>
        /// <param name="user"> The user making the request. </param>
        /// <param name="projectId"> The id of the project. </param>
        /// <param name="dataPointIndex"> The index of the data point. </param>
        /// <returns> A text data point. </returns>
        Task<ReadTextDataPointModel> GetTextDataPointAsync(User user, int projectId, int dataPointIndex);
        
        /// <summary>
        /// Labels a data point. 
        /// </summary>
        /// <param name="user"> The user who labels the data point. </param>
        /// <param name="projectId"> The project id of the project containing the data point. </param>
        /// <param name="dataPointIndex"> The index of the data point. </param>
        /// <param name="labelId"> The label, which will be assigend to the labeled data point. </param>
        /// <returns></returns>
        Task LabelDataPointsAsync(User user, int projectId, int dataPointIndex, int labelId);
        
        /// <summary>
        /// Updates an image data point.
        /// </summary>
        /// <param name="project"> The project. </param>
        /// <param name="dataPointIndex"> The data point index. </param>
        /// <param name="file"> The image. </param>
        /// <returns></returns>
        Task UpdateImageDataPointAsync(Project project, int dataPointIndex, IFormFile file);

        /// <summary>
        /// Updates a text data point. 
        /// </summary>
        /// <param name="project"> The project. </param>
        /// <param name="dataPointIndex"> The data point index. </param>
        /// <param name="modifiedTextDataPoint"> The updated text data point. </param>
        /// <returns></returns>
        Task UpdateTextDataPointAsync(Project project, int dataPointIndex, EditTextDataPointModel modifiedTextDataPoint);

        Task<List<ReadImageDataPointModel>> GetImageDataPointRangeAsync(int projectId, User user, int? startIndex = null, int? endIndex = null);
        Task<List<ReadTextDataPointModel>> GetTextDataPointRangeAsync(int projectId, User user, int? startIndex = null, int? endIndex = null);
    }
}