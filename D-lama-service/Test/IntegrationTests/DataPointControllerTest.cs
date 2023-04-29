using d_lama_service.Models.ProjectModels;
using d_lama_service.Models.UserViewModels;
using Data.ProjectEntities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace Test.IntegrationTests
{
    [TestClass]
    public class DataPointControllerTest : APITestBase
    {
        private readonly string _apiRoute = "api/DataPoint";
        private string _testFilesPath;
        private Project? _adminProject;

        public DataPointControllerTest()
        {
            var workingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            _testFilesPath = Path.Combine(workingDir, "IntegrationTests", "TestFiles");
        }

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
            await ClearTextDataPoints();
            await CleanupProjects();
        }

        [TestMethod]
        public async Task GetAllTextDataPoints_NoLogin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetAllTextDataPoints_InvalidId_ReturnsNotFound()
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
        public async Task GetAllTextDataPoints_DataPointsPresent_ReturnsOK()
        {
            // Arrange
            await AddSomeTextDataPoints(3);
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
        public async Task GetAllTextDataPoints_Empty_ReturnsNotFound()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetNumberOfTextDataPoints_NoLogin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/GetNumberOfTextDataPointsAsync";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetNumberOfTextDataPoints_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var uri = _apiRoute + "/-1" + "/GetNumberOfTextDataPointsAsync";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetNumberOfTextDataPoints_NoDataPointsPresent_ReturnsOK()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/GetNumberOfTextDataPointsAsync";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            var numberOfDataPoints = JsonConvert.DeserializeObject<int>(responseContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, numberOfDataPoints);
        }

        [TestMethod]
        public async Task GetNumberOfTextDataPoints_DataPointsPresent_ReturnsOK()
        {
            // Arrange
            await AddSomeTextDataPoints(3);
            var uri = _apiRoute + "/" + _adminProject.Id + "/GetNumberOfTextDataPointsAsync";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            var numberOfDataPoints = JsonConvert.DeserializeObject<int>(responseContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(3, numberOfDataPoints);
        }

        [TestMethod]
        public async Task GetTextDataPointByIndex_NoLogin_ReturnsUnauthorized()
        {
            // Arrange
            await AddSomeTextDataPoints(3);
            var uri = _apiRoute + "/" + _adminProject.Id + "/" + 1;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetTextDataPointByIndex_DataPointsPresent_ReturnsOK()
        {
            // Arrange
            await AddSomeTextDataPoints(3);
            var uri = _apiRoute + "/" + _adminProject.Id + "/" + 1;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetTextDataPointByIndex_Empty_ReturnsNotFound()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/" + 1;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetTextDataPointRange_NoLogin_ReturnsUnauthorized()
        {
            // Arrange
            await AddSomeTextDataPoints(4);
            var uri = _apiRoute + "/" + _adminProject.Id + "/" + 1 + "/" + 2;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetTextDataPointRange_DataPointsPresent_ReturnsOK()
        {
            // Arrange
            await AddSomeTextDataPoints(4);
            var uri = _apiRoute + "/" + _adminProject.Id + "/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetTextDataPointRange_Empty_ReturnsNotFound()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateSingleTextDataPoint_NoLogin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/CreateSingleTextDataPointAsync";
            var content = new StringContent(JsonConvert.SerializeObject(
                new TextDataPointModel { Content = "Content of a new data point." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateSingleTextDataPoint_NonAdmin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/CreateSingleTextDataPointAsync";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var content = new StringContent(JsonConvert.SerializeObject(
                new TextDataPointModel { Content = "Content of a new data point." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateSingleTextDataPoint_WrongAdmin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/CreateSingleTextDataPointAsync";
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            var content = new StringContent(JsonConvert.SerializeObject(
                new TextDataPointModel { Content = "Content of a new data point." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateSingleTextDataPoint_FirstEntry_ReturnsCreated()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/CreateSingleTextDataPointAsync";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(
                new TextDataPointModel { Content = "Content of a new data point." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateSingleTextDataPoint_AppendToExisting_ReturnsCreated()
        {
            // Arrange
            await AddSomeTextDataPoints(3);
            var uri = _apiRoute + "/" + _adminProject.Id + "/CreateSingleTextDataPointAsync";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(
                new TextDataPointModel { Content = "Content of a new data point." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateSingleTextDataPoint_WrongProjectId_ReturnsNotFound()
        {
            // Arrange
            await AddSomeTextDataPoints(3);
            var uri = _apiRoute + "/" + (-1) + "/CreateSingleTextDataPointAsync";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(
                new TextDataPointModel { Content = "Content of a new data point." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task UploadTextDataPoints_NoLogin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/UploadTextDataPointsAsync";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task UploadTextDataPoints_NonAdmin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/UploadTextDataPointsAsync";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task UploadTextDataPoints_WrongAdmin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/UploadTextDataPointsAsync";
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            var filePath = Path.Combine(_testFilesPath, "txt", "test_file_3_lines.txt");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task UploadTextDataPoints_UnsupportedFile_ReturnsBadRequest()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/UploadTextDataPointsAsync";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "unsupported", "test_file.xlsx");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task UploadTextDataPoints_ValidTxtFile_ReturnsOK()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/UploadTextDataPointsAsync";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "txt", "test_file_3_lines.txt");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task UploadTextDataPoints_ValidCsvFile_ReturnsOK()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/UploadTextDataPointsAsync";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "csv", "test_file_3_lines.csv");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task UploadTextDataPoints_ValidJsonFile_ReturnsOK()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/UploadTextDataPointsAsync";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "json", "test_file_3_lines.json");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task EditTextDataPoint_NoLogin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/EditTextDataPointAsync/" + 2;
            var content = new StringContent(JsonConvert.SerializeObject(
                new EditTextDataPointModel { Content = "My new data point content." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task EditTextDataPoint_NonAdmin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/EditTextDataPointAsync/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var content = new StringContent(JsonConvert.SerializeObject(
                new EditTextDataPointModel { Content = "My new data point content." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task EditTextDataPoint_WrongAdmin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/EditTextDataPointAsync/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            var content = new StringContent(JsonConvert.SerializeObject(
                new EditTextDataPointModel { Content = "My new data point content." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task EditTextDataPoint_CorrectAdmin_ReturnsOK()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/EditTextDataPointAsync/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(
                new EditTextDataPointModel { Content = "My new data point content." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task EditTextDataPoint_WrongProjectId_ReturnsNotFound()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + (-1) + "/EditTextDataPointAsync/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(
                new EditTextDataPointModel { Content = "My new data point content." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task EditTextDataPoint_WrongDataPointId_ReturnsNotFound()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/EditTextDataPointAsync/" + (-1);
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(
                new EditTextDataPointModel { Content = "My new data point content." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task EditTextDataPoint_NoDataPointsPresent_ReturnsNotFound()
        {
            await ClearTextDataPoints();
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/EditTextDataPointAsync/" + (1);
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(
                new EditTextDataPointModel { Content = "My new data point content." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAllTextDataPoints_NoLogin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/DeleteTextDataPoints";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAllTextDataPoints_NonAdmin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/DeleteTextDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAllTextDataPoints_WrongAdmin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/DeleteTextDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAllTextDataPoints_CorrectAdmin_ReturnsOK()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/DeleteTextDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAllTextDataPoints_NoDataPointsPresent_ReturnsNotFound()
        {
            await ClearTextDataPoints();
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/DeleteTextDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAllTextDataPoints_WrongProjectId_ReturnsNotFound()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + (-1) + "/DeleteTextDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTextDataPointRange_NoLogin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/DeleteTextDataPoints/" + 1 + "/" + 2;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTextDataPointRange_NonAdmin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/DeleteTextDataPoints/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTextDataPointRange_WrongAdmin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/DeleteTextDataPoints/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTextDataPointRange_CorrectAdmin_ReturnsOK()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/DeleteTextDataPoints/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTextDataPointRange_NoDataPointsPresent_ReturnsNotFound()
        {
            await ClearTextDataPoints();
            // Arrange
            var uri = _apiRoute + "/" + _adminProject.Id + "/DeleteTextDataPoints/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTextDataPointRange_WrongProjectId_ReturnsNotFound()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + (-1) + "/DeleteTextDataPoints/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task SetUpProjects()
        {
            _adminProject = new Project("AdminProject", "My Description");
            _adminProject.Labels.Add(new Label("TestSet", "TestDesc"));
            var adminUser = await Context.Users.Where(u => u.Email == Admin.Email).FirstOrDefaultAsync();
            adminUser?.Projects.Add(_adminProject);
            await Context.AddAsync(_adminProject);
            await Context.SaveChangesAsync();
        }

        private async Task CleanupProjects()
        {
            try { ReloadContext(); } catch { }
            var adminProj = await Context.Projects.FindAsync(_adminProject.Id);
            if (adminProj != null)
            {
                Context.Remove(adminProj);
                await Context.SaveChangesAsync();
            }
        }

        private async Task AddSomeTextDataPoints(int numberOfDataPoints)
        {
            string content = "Text content for data point ";
            for (int i = 0; i < numberOfDataPoints; i++)
            {
                var dataPoint = new TextDataPoint(content + i, i);
                await Context.AddAsync(dataPoint);
                _adminProject.TextDataPoints.Add(dataPoint);
            }
            Context.Update(_adminProject);
            await Context.SaveChangesAsync();
        }

        private async Task ClearTextDataPoints()
        {
            var dataPoints = await Context.TextDataPoints.Where(e => e.ProjectId == _adminProject.Id).ToListAsync();
            if (dataPoints.Any())
            {
                foreach (var dataPoint in dataPoints)
                {
                    Context.Remove(dataPoint);
                }

                Context.Update(_adminProject);
                await Context.SaveChangesAsync();
            }
        }

        private MultipartFormDataContent GetMultipartFormDataContent(string filePath)
        {
            var file = File.OpenRead(filePath);
            var content = new StreamContent(file);
            var formData = new MultipartFormDataContent();
            // Add file (file, field name, file name)
            formData.Add(content, "uploadedFile", file.Name);
            return formData;
        }
    }
}
