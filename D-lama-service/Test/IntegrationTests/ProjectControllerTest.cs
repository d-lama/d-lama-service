using d_lama_service.Models.ProjectModels;
using d_lama_service.Models.UserViewModels;
using Data.ProjectEntities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Test.IntegrationTests
{
    [TestClass]
    public class ProjectControllerTest : APITestBase
    {
        private readonly string _apiRoute = "api/Project";
        private string _testProjectName = "TestProjectName";
        private Project _adminProject;

        /// <summary>
        /// Before each run.
        /// </summary>
        [TestInitialize]
        public async Task BeforeEach()
        {
            await SetUpProjects();
        }

        /// <summary>
        /// After each run.
        /// </summary>
        [TestCleanup]
        public async Task AfterEach()
        {
            await CleanupProjects();
        }

        [TestMethod]
        public async Task GetAll_Unauthorized() 
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiRoute);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetAll_Authorized()
        {
            // Arrange
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiRoute);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetMyProjects_NonAdmin() 
        {
            // Arrange
            var uri = _apiRoute + "/My";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetMyProjects_Admin()
        {
            // Arrange
            var uri = _apiRoute + "/My";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetProject_InvalidId()
        {
            // Arrange
            var uri = _apiRoute + "/-1";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetProject_ValidId_User()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetProject_ValidId_Admin()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateProject_NonAdmin()
        {
            // Arrange
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var content = new StringContent(JsonConvert.SerializeObject(new ProjectModel { ProjectName = _testProjectName, Description = "Test", LabelSets = new List<LabelSetChangeModel> { } }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiRoute);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateProject_InvalidRequest()
        {
            // Arrange
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(new ProjectModel { ProjectName = _testProjectName }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiRoute);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateProject_ValidRequest()
        {
            // Arrange
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(new ProjectModel { ProjectName = _testProjectName, Description = "Test", LabelSets = new List<LabelSetChangeModel> { } }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiRoute);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateProject_ValidRequest_TwiceSameName()
        {
            // Arrange
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(new ProjectModel { ProjectName = _testProjectName, Description = "Test", LabelSets = new List<LabelSetChangeModel> { } }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiRoute);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            HttpRequestMessage request2 = new HttpRequestMessage(HttpMethod.Post, _apiRoute);
            request2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request2.Content = content;

            // Act
            var response = await Client.SendAsync(request);
            var response2 = await Client.SendAsync(request2);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(HttpStatusCode.BadRequest, response2.StatusCode);
        }

        [TestMethod]
        public async Task EditProject_NotAdmin()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var content = new StringContent(JsonConvert.SerializeObject(new EditProjectModel { ProjectName = _testProjectName, Description = "Test" }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task EditProject_InvalidId()
        {
            // Arrange
            var uri = _apiRoute + "/-1";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(new EditProjectModel { ProjectName = _testProjectName, Description = "Test" }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task EditProject_WrongAdmin()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id;
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            var content = new StringContent(JsonConvert.SerializeObject(new EditProjectModel { ProjectName = _testProjectName, Description = "Test" }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task EditProject_CorrectAdmin_Description()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(new EditProjectModel {Description = "My New Description" }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task EditProject_CorrectAdmin_Nothing()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(new EditProjectModel { }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task EditProject_CorrectAdmin_LabelSet()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var labelSetChanges = new LabelSetChangeModel { Id = _adminProject.LabelSets.First().Id, Description = "..." };
            var content = new StringContent(JsonConvert.SerializeObject(new EditProjectModel { LabeSetChanges = new LabelSetChangeModel[] { labelSetChanges } }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteProject_NotAdmin()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteProject_WrongAdmin()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id;
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteProject_InvalidId()
        {
            // Arrange
            var uri = _apiRoute + "/-1";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteProject_CorrectAdmin()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task AddLabels_InvalidRequest()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/Labels";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(null), Encoding.UTF8, "application/json");
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task AddLabels_WrongAdmin()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/Labels";
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(new LabelSetModel[] { new LabelSetModel { Name = "My new Label" } }), Encoding.UTF8, "application/json");
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task AddLabels_ValidRequest()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/Labels";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(new LabelSetModel[] { new LabelSetModel { Name = "My new Label" } }), Encoding.UTF8, "application/json");
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }


        [TestMethod]
        public async Task RemoveLabels_WrongAdmin()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/Labels/" + _adminProject.LabelSets.First().Id;
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task RemoveLabels_InvalidId()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/Labels/-1";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task RemoveLabels_ValidId()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/Labels/" + _adminProject.LabelSets.First().Id;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task SetUpProjects() 
        {
            _adminProject = new Project("AdminProject","My Description");
            _adminProject.LabelSets.Add(new LabelSet("TestSet", "TestDesc"));
            Admin.Projects.Add(_adminProject);
            await Context.AddAsync(_adminProject);
            await Context.SaveChangesAsync();
        }

        private async Task CleanupProjects() 
        {
            try { ReloadContext(); } catch { }
            var testProject = await Context.Projects.Where(e => e.ProjectName == _testProjectName).FirstOrDefaultAsync();
            if (testProject != null)
            {
                Context.Remove(testProject);
            }
            var adminProj = await Context.Projects.FindAsync(_adminProject.Id);
            if (adminProj != null)
            {
                Context.Remove(adminProj);
                await Context.SaveChangesAsync();
            }
        }
    }
}
