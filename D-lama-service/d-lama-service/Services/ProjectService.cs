using d_lama_service.Models;
using d_lama_service.Models.ProjectModels;
using d_lama_service.Models.UserModels;
using d_lama_service.Repositories;
using d_lama_service.Repositories.DataPointRepositories;
using d_lama_service.Repositories.ProjectRepositories;
using d_lama_service.Repositories.UserRepositories;
using Data;
using Data.ProjectEntities;
using System.Net;

namespace d_lama_service.Services
{
    /// <summary>
    /// Implementation of the IProjectService.
    /// The project service handles the domain specific logic regarding the project.
    /// </summary>
    public class ProjectService : Service, IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly ILabeledDataPointRepository _labeledDataPointRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Constructor of ProjectService.
        /// </summary>
        /// <param name="context"> The database context. </param>
        /// <param name="environment"> The environment. </param>
        public ProjectService(DataContext context, IWebHostEnvironment environment) : base(context)
        {
            _projectRepository = new ProjectRepository(context);
            _labelRepository = new LabelRepository(context);    
            _labeledDataPointRepository = new LabeledDataPointRepository(context);
            _userRepository = new UserRepository(context);  
            _environment = environment;
        }

        public async Task<int> CreateProjectAsync(User user, ProjectModel projectForm) 
        {
            await ValidateProjectName(projectForm.ProjectName);

            Project project;
            switch (projectForm.DataType) 
            {
                case ProjectDataType.Image:
                    string webRootPath = _environment.WebRootPath.ToString();
                    string projectDirectoryPath = Path.Combine(webRootPath, "project_files");
                    project = new Project(projectForm.ProjectName, projectForm.Description, projectDirectoryPath);
                    break;
                case ProjectDataType.Text:
                    project = new Project(projectForm.ProjectName, projectForm.Description);
                    break;
                default:
                    throw new RESTException(HttpStatusCode.BadRequest, "Unsupported data type. The following data types are supported: text (= 0), image (= 1).");
            }

            user.Projects.Add(project);
            foreach (var label in projectForm.Labels)
            {
                project.Labels.Add(new Label(label.Name, label.Description));
            }

            return await SaveProjectAsync(project);
        }

        public async Task UpdateProjectAsync(Project project, EditProjectModel modifiedProject) 
        {
            if (modifiedProject.Name != null && project.Name != modifiedProject.Name) 
            {
                await ValidateProjectName(modifiedProject.Name);
            } 

            var labeSetChanges = modifiedProject.LabeSetChanges;
            if (labeSetChanges != null)
            {
                foreach (var change in labeSetChanges)
                {
                    UpdateLabel(project, change);
                }
            }

            project.Name = modifiedProject.Name ?? project.Name;
            project.Description = modifiedProject.Description ?? project.Description;
            project.UpdateDate = DateTime.UtcNow;

            _projectRepository.Update(project);
            await SaveAsync();
        }

        public async Task<List<Project>> GetProjectsOfOwnerAsync(int ownerId)
        {
            return (await _projectRepository.FindAsync(e => e.OwnerId == ownerId)).ToList();
        }

        public async Task<List<DetailedProjectModel>> GetAllProjectsAsync(User user) 
        {
            var projectList = new List<DetailedProjectModel>();
            IEnumerable<Project> projects = await _projectRepository.GetAllAsync();
            foreach (var project in projects)
            {
                var detailedProject = await GetDetailedProjectAsync(project, user.Id);
                projectList.Add(detailedProject);
            }
            return projectList;
        }

        public async Task<DetailedProjectModel> GetProjectAsync(int id, User user) 
        {
            var project = await _projectRepository.GetDetailsAsync(id, e => e.Labels, e => e.DataPoints);

            if (project == null)
            {
                throw new RESTException(HttpStatusCode.NotFound,"");
            }

            return await GetDetailedProjectAsync(project, user.Id);
        }

        public async Task DeleteProjectAsync(Project project) 
        {
            if (project.DataType == ProjectDataType.Image)
            {
                Directory.Delete(project.StoragePath, true);
            }

            _projectRepository.Delete(project);
            await SaveAsync();
        }

        public async Task CreateLablesAsync(Project project, LabelSetModel[] newLabels) 
        {
            foreach (var label in newLabels)
            {
                if (project.Labels.Select(e => e.Name).Contains(label.Name))
                {
                    await DisposeAsync();
                    throw new RESTException(HttpStatusCode.BadRequest, $"A label with the name '{label.Name}' does already exists. Please use another name.");
                }
                project.Labels.Add(new Label(label.Name, label.Description));
            }

            _projectRepository.Update(project);
            await SaveAsync();
        }

        public async Task DeleteLabelAsync(int projectId, int labelId) 
        {
            var label = await _labelRepository.GetAsync(labelId);
            if (label == null || label.ProjectId != projectId)
            {
                throw new RESTException(HttpStatusCode.NotFound, "");
            }

            var labeledData = await _labeledDataPointRepository.FindAsync(e => e.LabelId == labelId);
            if (labeledData.Any())
            {
                throw new RESTException(HttpStatusCode.BadRequest,"Label was already used and cannot be deleted therefore.");
            }

            _labelRepository.Delete(label);
            await SaveAsync();
        }

        public async Task<List<UserRankingModel>> GetProjectRankingList(Project project) 
        {
            var dataPointIds = project.DataPoints.Select(e => e.Id);
            if (dataPointIds == null || !dataPointIds.Any())
            {
                throw new RESTException(HttpStatusCode.BadRequest, "You need to add DataPoints first, before you can get a ranking!");
            }

            var users = await _userRepository.GetAllAsync();
            var ranking = new List<UserRankingModel>();
            foreach (var user in users)
            {
                var labeledPointsCount = (await _labeledDataPointRepository.FindAsync(e => e.UserId == user.Id && dataPointIds.Contains(e.DataPointId))).Count();
                var percentage = (float)labeledPointsCount / (float)dataPointIds.Count();
                ranking.Add(new UserRankingModel(user.Id, user.FirstName + " " + user.LastName, percentage));
            }

            return ranking.OrderByDescending(e => e.Percentage).ToList();
        }

        private async Task<DetailedProjectModel> GetDetailedProjectAsync(Project project, int userId) 
        {
            var detailedProject = await _projectRepository.GetDetailsAsync(project.Id, e => e.DataPoints);
            var dataPointsIds = project.DataPoints.Select(e => e.Id).ToList();
            var labeledFromUser = await _labeledDataPointRepository.FindAsync(e => e.UserId == userId && dataPointsIds.Contains(e.DataPointId));
            return new DetailedProjectModel(project, dataPointsIds.Count, labeledFromUser.Count());
        }

        private async Task ValidateProjectName(string? projectName)
        {
            var nameExists = (await _projectRepository.FindAsync(e => e.Name == projectName)).Any();
            if (nameExists)
            {
                throw new RESTException(HttpStatusCode.BadRequest, "A project with this name has already been created.");
            }
        }

        private async Task<int> SaveProjectAsync(Project project)
        {
            // save changes needed in order to get Id
            _projectRepository.Update(project);
            await SaveAsync();

            if (project.DataType == ProjectDataType.Image)
            {
                var projectDirectoryPath = Path.Combine(project.StoragePath, $"project_{project.Id}");
                Directory.CreateDirectory(projectDirectoryPath);
                project.StoragePath = projectDirectoryPath;

                _projectRepository.Update(project);
                await SaveAsync();
            }
            return project.Id;
        }

        private void UpdateLabel(Project project, LabelChangeModel labelChange)
        {
            var label = project.Labels.Where(e => e.Id == labelChange.Id).FirstOrDefault();
            if (label == null)
            {
                throw new RESTException(HttpStatusCode.NotFound, $"Label with id {labelChange.Id} not found.");
            }
            label.Name = labelChange.Name ?? label.Name;
            label.Description = labelChange.Description ?? label.Description;
        }
    }
}
