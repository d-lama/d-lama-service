using d_lama_service.Models.ProjectModels;
using d_lama_service.Models.UserModels;
using Data;
using Data.ProjectEntities;

namespace d_lama_service.Services
{
    /// <summary>
    /// Interface of the ProjectService.
    /// </summary>
    public interface IProjectService : IService
    {
        /// <summary>
        /// Creates a project from a project creation request.
        /// </summary>
        /// <param name="user"> The user who wants to create the project. </param>
        /// <param name="projectForm"> The project creation request. </param>
        /// <returns> The id of the new created project. </returns>
        Task<int> CreateProjectAsync(User user, ProjectModel projectForm);

        /// <summary>
        /// Gets a list of the projects from an owner.
        /// </summary>
        /// <param name="ownerId"> The id of the user (owner). </param>
        /// <returns> A list of projects. If nothing found, an empty list. </returns>
        Task<List<Project>> GetProjectsOfOwnerAsync(int ownerId);

        /// <summary>
        /// Gets all available projects.
        /// </summary>
        /// <param name="user"> The user making the request. </param>
        /// <returns> A list with all projects. If nothing found, an empty list. </returns>
        Task<List<DetailedProjectModel>> GetAllProjectsAsync(User user);

        /// <summary>
        /// Gets a project.
        /// </summary>
        /// <param name="id"> The id of the project. </param>
        /// <param name="user"> The user making the request. </param>
        /// <returns> The project. </returns>
        Task<DetailedProjectModel> GetProjectAsync(int id, User user);

        /// <summary>
        /// Updates a Project. 
        /// </summary>
        /// <param name="project"> The project to update. </param>
        /// <param name="modifiedProject"> The new project (the update). </param>
        /// <returns></returns>
        Task UpdateProjectAsync(Project project, EditProjectModel modifiedProject);

        /// <summary>
        /// Deletes a project.
        /// </summary>
        /// <param name="project"> The project to delete. </param>
        /// <returns></returns>
        Task DeleteProjectAsync(Project project);

        /// <summary>
        /// Creates labels for a project.
        /// </summary>
        /// <param name="project"> The project, where the label should be added. </param>
        /// <param name="newLabels"> The new labels to add to the project. </param>
        /// <returns></returns>
        Task CreateLablesAsync(Project project, LabelSetModel[] newLabels);

        /// <summary>
        /// Deletes a label for a project.
        /// </summary>
        /// <param name="projectId"> The id of the project, where the label should be deleted. </param>
        /// <param name="labelId"> The id of the label to be deleted. </param>
        /// <returns></returns>
        Task DeleteLabelAsync(int projectId, int labelId);

        /// <summary>
        /// Gets a ranking list for the project.
        /// </summary>
        /// <param name="project"> The project of which the ranking is wanted. </param>
        /// <returns> The ranking list for the project. </returns>
        Task<List<UserRankingModel>> GetProjectRankingList(Project project);
    }
}
