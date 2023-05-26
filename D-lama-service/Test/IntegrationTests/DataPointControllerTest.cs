using d_lama_service.Models.ProjectModels;
using d_lama_service.Models.UserModels;
using Data.ProjectEntities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO.Compression;
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
        private readonly string _testProjectDirectoryPath;
        private readonly string _testFilesPath;
        private Project _adminProjectText;
        private Project _adminProjectImage;

        public DataPointControllerTest()
        {
            string workingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            _testFilesPath = Path.Combine(workingDir ?? string.Empty, "IntegrationTests", "TestFiles");
            _testProjectDirectoryPath = Path.Combine(workingDir ?? string.Empty, "test_project_files");
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
            await ClearImageDataPoints();
            await CleanupProjects();
        }

        [TestMethod]
        public async Task GetAll_TextNoLogin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetAll_ImageNoLogin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetAll_InvalidId_ReturnsNotFound()
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
        public async Task GetAll_TextDataPointsPresent_ReturnsOK()
        {
            // Arrange
            await AddSomeTextDataPoints(3);
            var uri = _apiRoute + "/" + _adminProjectText.Id;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetAll_ImageDataPointsPresent_ReturnsOK()
        {
            // Arrange
            await AddSomeImageDataPoints();
            var uri = _apiRoute + "/" + _adminProjectImage.Id;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetAll_Empty_ReturnsOk()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetCount_NoLogin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/GetNumberOfDataPoints";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetCount_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var uri = _apiRoute + "/-1" + "/GetNumberOfDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetCount_NoDataPointsPresent_ReturnsOK()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/GetNumberOfDataPoints";
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
        public async Task GetCount_TextDataPointsPresent_ReturnsOK()
        {
            // Arrange
            await AddSomeTextDataPoints(6);
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/GetNumberOfDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            var numberOfDataPoints = JsonConvert.DeserializeObject<int>(responseContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(6, numberOfDataPoints);
        }

        [TestMethod]
        public async Task GetCount_ImageDataPointsPresent_ReturnsOK()
        {
            // Arrange
            await AddSomeImageDataPoints();
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/GetNumberOfDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            var numberOfDataPoints = JsonConvert.DeserializeObject<int>(responseContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(6, numberOfDataPoints);
        }

        [TestMethod]
        public async Task Get_NoLogin_ReturnsUnauthorized()
        {
            // Arrange
            await AddSomeTextDataPoints(3);
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/" + 1;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_TextDataPointsPresent_ReturnsOK()
        {
            // Arrange
            await AddSomeTextDataPoints(3);
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/" + 1;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_ImageDataPointsPresent_ReturnsOK()
        {
            // Arrange
            await AddSomeImageDataPoints();
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/" + 1;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_Empty_ReturnsNotFound()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/" + 1;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetRange_NoLogin_ReturnsUnauthorized()
        {
            // Arrange
            await AddSomeTextDataPoints(4);
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/" + 1 + "/" + 2;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task GetRange_TextDataPointsPresent_ReturnsOK()
        {
            // Arrange
            await AddSomeTextDataPoints(4);
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetRange_ImageDataPointsPresent_ReturnsOK()
        {
            // Arrange
            await AddSomeImageDataPoints();
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetRange_Empty_ReturnsOk()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateTextPoint_NoLogin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/CreateSingleTextDataPoint";
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
        public async Task CreateTextPoint_NonAdmin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/CreateSingleTextDataPoint";
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
        public async Task CreateTextPoint_WrongAdmin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/CreateSingleTextDataPoint";
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
        public async Task CreateTextPoint_FirstEntry_ReturnsCreated()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/CreateSingleTextDataPoint";
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
        public async Task CreateTextPoint_AppendToExisting_ReturnsCreated()
        {
            // Arrange
            await AddSomeTextDataPoints(3);
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/CreateSingleTextDataPoint";
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
        public async Task CreateTextPoint_WrongProjectId_ReturnsNotFound()
        {
            // Arrange
            await AddSomeTextDataPoints(3);
            var uri = _apiRoute + "/" + (-1) + "/CreateSingleTextDataPoint";
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
        public async Task CreateImagePoint_NoLogin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadSingleImageDataPoint";
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateImagePoint_NonAdmin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadSingleImageDataPoint";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
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
        public async Task CreateImagePoint_WrongAdmin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadSingleImageDataPoint";
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
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
        public async Task CreateImagePoint_Jpg100KbFirstEntry_ReturnsCreated()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadSingleImageDataPoint";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateImagePoint_Png500KbFirstEntry_ReturnsCreated()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadSingleImageDataPoint";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "png", "file_example_PNG_500kB.png");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateImagePoint_Jpeg1MbFirstEntry_ReturnsCreated()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadSingleImageDataPoint";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpeg", "file_example_JPEG_1MB.jpeg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateImagePoint_Jpg15MbFirstEntry_ReturnsCreated()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadSingleImageDataPoint";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_15MB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateImagePoint_Jpg100KbAppendToExisting_ReturnsCreated()
        {
            // Arrange
            await AddSomeImageDataPoints();
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadSingleImageDataPoint";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateImagePoint_Png500KbAppendToExisting_ReturnsCreated()
        {
            // Arrange
            await AddSomeImageDataPoints();
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadSingleImageDataPoint";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "png", "file_example_PNG_500kB.png");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateImagePoint_Jpeg1MbAppendToExisting_ReturnsCreated()
        {
            // Arrange
            await AddSomeImageDataPoints();
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadSingleImageDataPoint";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpeg", "file_example_JPEG_1MB.jpeg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateImagePoint_Jpg15MbAppendToExisting_ReturnsCreated()
        {
            // Arrange
            await AddSomeImageDataPoints();
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadSingleImageDataPoint";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_15MB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateImagePoint_WrongProjectId_ReturnsNotFound()
        {
            // Arrange
            await AddSomeTextDataPoints(3);
            var uri = _apiRoute + "/" + (-1) + "/UploadSingleImageDataPoint";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task UploadMultipleImageDataPoints_Jpg100Kb_ReturnsCreated()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadSingleImageDataPoint";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            for (int i = 0; i < 1000; i++)
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = content;

                // Act
                var response = await Client.SendAsync(request);

                // Assert
                Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            }
        }

        [TestMethod]
        public async Task CreateMutlipleTextPoints_NoLogin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/UploadTextDataPoints";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateMutlipleTextPoints_NonAdmin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/UploadTextDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateMutlipleTextPoints_WrongAdmin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/UploadTextDataPoints";
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
        public async Task CreateMutlipleTextPoints_UnsupportedFile_ReturnsBadRequest()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/UploadTextDataPoints";
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
        public async Task CreateMutlipleTextPoints_ValidTxtFile_ReturnsOK()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/UploadTextDataPoints";
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
        public async Task CreateMutlipleTextPoints_ValidCsvFile_ReturnsOK()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/UploadTextDataPoints";
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
        public async Task CreateMutlipleTextPoints_ValidJsonFile_ReturnsOK()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/UploadTextDataPoints";
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
        public async Task CreateMultipleImagePoints_NoLogin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadImageDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);


            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateMultipleImagePoints_NonAdmin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadImageDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateMultipleImagePoints_WrongAdmin_ReturnsUnauthorized()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadImageDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "zip_example_JPG_100kB_6_files.zip");
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
        public async Task CreateMultipleImagePoints_UnsupportedFile_ReturnsBadRequest()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadImageDataPoints";
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
        public async Task CreateMultipleImagePoints_ValidZipFileJpg_ReturnsOK()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadImageDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "zip_example_JPG_100kB_6_files.zip");
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
        public async Task CreateMultipleImagePoints_ValidZipFilePng_ReturnsOK()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadImageDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "png", "zip_example_PNG_500kB_6_files.zip");
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
        public async Task CreateMultipleImagePoints_ValidZipFileJpeg_ReturnsOK()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/UploadImageDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpeg", "zip_example_JPEG_500kB_6_files.zip");
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
        public async Task EditTextPoint_NoLogin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/EditTextDataPoint/" + 2;
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
        public async Task EditTextPoint_NonAdmin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/EditTextDataPoint/" + 2;
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
        public async Task EditTextPoint_WrongAdmin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/EditTextDataPoint/" + 2;
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
        public async Task EditTextPoint_WrongDataType_ReturnsBadRequest()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/EditTextDataPoint/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var content = new StringContent(JsonConvert.SerializeObject(
                new EditTextDataPointModel { Content = "My new data point content." }), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task EditTextPoint_CorrectAdmin_ReturnsOK()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/EditTextDataPoint/" + 2;
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
        public async Task EditTextPoint_WrongProjectId_ReturnsNotFound()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + (-1) + "/EditTextDataPoint/" + 2;
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
        public async Task EditTextPoint_WrongDataPointId_ReturnsNotFound()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/EditTextDataPoint/" + (-1);
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
        public async Task EditTextPoint_NoDataPointsPresent_ReturnsNotFound()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/EditTextDataPoint/" + (1);
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
        public async Task EditImagePoint_NoLogin_ReturnsUnauthorized()
        {
            await AddSomeImageDataPoints();
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/EditImageDataPoint/" + 2;
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task EditImagePoint_NonAdmin_ReturnsUnauthorized()
        {
            await AddSomeImageDataPoints();
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/EditImageDataPoint/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task EditImagePoint_WrongAdmin_ReturnsUnauthorized()
        {
            await AddSomeImageDataPoints();
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/EditImageDataPoint/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task EditImagePoint_WrongDataType_ReturnsBadRequest()
        {
            await AddSomeImageDataPoints();
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/EditImageDataPoint/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task EditImagePoint_CorrectAdmin_ReturnsOK()
        {
            await AddSomeImageDataPoints();
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/EditImageDataPoint/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task EditImagePoint_WrongProjectId_ReturnsNotFound()
        {
            await AddSomeImageDataPoints();
            // Arrange
            var uri = _apiRoute + "/" + (-1) + "/EditImageDataPoint/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task EditImagePoint_WrongDataPointId_ReturnsNotFound()
        {
            await AddSomeImageDataPoints();
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/EditImageDataPoint/" + (-1);
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task EditImagePoint_NoDataPointsPresent_ReturnsNotFound()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/EditImageDataPoint/" + (1);
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            var filePath = Path.Combine(_testFilesPath, "jpg", "file_example_JPG_100kB.jpg");
            var content = GetMultipartFormDataContent(filePath);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAll_NoLogin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/DeleteDataPoints";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAll_NonAdmin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/DeleteDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAll_WrongAdmin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/DeleteDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAll_TextCorrectAdmin_ReturnsOK()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/DeleteDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAll_ImageCorrectAdmin_ReturnsOK()
        {
            await AddSomeImageDataPoints();
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/DeleteDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAll_NoDataPointsPresent_ReturnsOk()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/DeleteDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteAll_WrongProjectId_ReturnsNotFound()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + (-1) + "/DeleteDataPoints";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTextPointRange_NoLogin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/DeleteDataPoints/" + 1 + "/" + 2;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTextPointRange_NonAdmin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/DeleteDataPoints/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTextPointRange_WrongAdmin_ReturnsUnauthorized()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/DeleteDataPoints/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTextPointRange_TextCorrectAdmin_ReturnsOK()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/DeleteDataPoints/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTextPointRange_ImageCorrectAdmin_ReturnsOK()
        {
            await AddSomeImageDataPoints();
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectImage.Id + "/DeleteDataPoints/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTextPointRange_NoDataPointsPresent_ReturnsOk()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/DeleteDataPoints/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTextPointRange_WrongProjectId_ReturnsNotFound()
        {
            await AddSomeTextDataPoints(4);
            // Arrange
            var uri = _apiRoute + "/" + (-1) + "/DeleteDataPoints/" + 1 + "/" + 2;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task LabelDataPoint_InvalidProject() 
        {
            // Arrange
            await AddSomeTextDataPoints(1);
            var project = await GetFullAdminProjectAsync();
            var validDataPointIndex = project.DataPoints.FirstOrDefault()!.DataPointIndex;
            var validLabelId = project.Labels.FirstOrDefault()!.Id;

            var uri = _apiRoute + "/" + (-1) + "/LabelDataPoint/" + validDataPointIndex;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var content = new StringContent(JsonConvert.SerializeObject(validLabelId), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }


        [TestMethod]
        public async Task LabelPoint_InvalidDataPointIndex()
        {
            // Arrange
            await AddSomeTextDataPoints(1);
            var project = await GetFullAdminProjectAsync();
            var validDataPointIndex = project.DataPoints.FirstOrDefault()!.DataPointIndex;
            var validLabelId = project.Labels.FirstOrDefault()!.Id;

            var uri = _apiRoute + "/" + project.Id + "/LabelDataPoint/" + -1;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var content = new StringContent(JsonConvert.SerializeObject(validLabelId), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }


        [TestMethod]
        public async Task LabelPoint_InvalidLabelId()
        {
            // Arrange
            await AddSomeTextDataPoints(1);
            var project = await GetFullAdminProjectAsync();
            var validDataPointIndex = project.DataPoints.FirstOrDefault()!.DataPointIndex;
            var validLabelId = project.Labels.FirstOrDefault()!.Id;

            var uri = _apiRoute + "/" + project.Id + "/LabelDataPoint/" + validDataPointIndex;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var content = new StringContent(JsonConvert.SerializeObject(-1), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }


        [TestMethod]
        public async Task LabelPoint_Valid()
        {
            // Arrange
            await AddSomeTextDataPoints(1);
            var project = await GetFullAdminProjectAsync();
            var validDataPointIndex = project.DataPoints.FirstOrDefault()!.DataPointIndex;
            var validLabelId = project.Labels.FirstOrDefault()!.Id;

            var uri = _apiRoute + "/" + project.Id + "/LabelDataPoint/" + validDataPointIndex;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var content = new StringContent(JsonConvert.SerializeObject(validLabelId), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }


        [TestMethod]
        public async Task LabelPoint_AlreadyLabeled()
        {
            // Arrange
            await AddSomeTextDataPoints(1);
            var project = await GetFullAdminProjectAsync();
            var validDataPointIndex = project.DataPoints.FirstOrDefault()!.DataPointIndex;
            var validLabelId = project.Labels.FirstOrDefault()!.Id;

            var uri = _apiRoute + "/" + project.Id + "/LabelDataPoint/" + validDataPointIndex;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var content = new StringContent(JsonConvert.SerializeObject(validLabelId), Encoding.UTF8, "application/json");
            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            HttpRequestMessage request2 = new HttpRequestMessage(HttpMethod.Post, uri);
            request2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request2.Content = content;

            // Act
            var response = await Client.SendAsync(request);
            var response2 = await Client.SendAsync(request2);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(HttpStatusCode.BadRequest, response2.StatusCode);

        }

        [TestMethod]
        public async Task RemoveLabeledPoint_InvalidProject()
        {
            // Arrange
            await AddSomeTextDataPoints(1);
            var project = await GetFullAdminProjectAsync();
            var validDataPointIndex = project.DataPoints.FirstOrDefault()!.DataPointIndex;
            var validLabelId = project.Labels.FirstOrDefault()!.Id;

            await LabelDataPointAsync(User, validDataPointIndex, validLabelId); // label the datapoint

            var uri = _apiRoute + "/" + -1 + "/LabelDataPoint/" + validDataPointIndex;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task RemoveLabeledPoint_InvalidDataPointIndex()
        {
            // Arrange
            await AddSomeTextDataPoints(1);
            var project = await GetFullAdminProjectAsync();
            var validDataPointIndex = project.DataPoints.FirstOrDefault()!.DataPointIndex;
            var validLabelId = project.Labels.FirstOrDefault()!.Id;

            await LabelDataPointAsync(User, validDataPointIndex, validLabelId); // label the datapoint

            var uri = _apiRoute + "/" + project.Id + "/LabelDataPoint/" + -1;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task RemoveLabeledPoint_WrongUser()
        {
            // Arrange
            await AddSomeTextDataPoints(1);
            var project = await GetFullAdminProjectAsync();
            var validDataPointIndex = project.DataPoints.FirstOrDefault()!.DataPointIndex;
            var validLabelId = project.Labels.FirstOrDefault()!.Id;

            await LabelDataPointAsync(User, validDataPointIndex, validLabelId); // label the datapoint

            var uri = _apiRoute + "/" + project.Id + "/LabelDataPoint/" + validDataPointIndex;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password }); // wrong user
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task RemoveLabeledPoint_Valid()
        {
            // Arrange
            await AddSomeTextDataPoints(1);
            var project = await GetFullAdminProjectAsync();
            var validDataPointIndex = project.DataPoints.FirstOrDefault()!.DataPointIndex;
            var validLabelId = project.Labels.FirstOrDefault()!.Id;
            
            await LabelDataPointAsync(User, validDataPointIndex, validLabelId); // label the datapoint

            var uri = _apiRoute + "/" + project.Id + "/LabelDataPoint/" + validDataPointIndex;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetLabeledData_InvalidProjectId() 
        {
            // Arrange
            var uri = _apiRoute + "/-1/GetLabeledData";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }


        [TestMethod]
        public async Task GetLabeledData_InvalidOwner()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/GetLabeledData";
            var token = await GetAuthToken(new LoginModel { Email = Admin2.Email, Password = Admin2.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }


        [TestMethod]
        public async Task GetLabeledData_ValidRequest_NoDataPoints()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/GetLabeledData";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }


        [TestMethod]
        public async Task GetLabeledData_ValidRequest()
        {
            // Arrange
            await AddSomeTextDataPoints(10);
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/GetLabeledData";
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task GetLabeledPoint_InvalidDataPointIndex()
        {
            // Arrange
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/GetLabeledData/" + -1;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetLabeledPoint_ValidRequest()
        {
            // Arrange
            await AddSomeTextDataPoints(1);
            var project = await GetFullAdminProjectAsync();
            var validDataPointIndex = project.DataPoints.FirstOrDefault()!.DataPointIndex;
            var uri = _apiRoute + "/" + _adminProjectText.Id + "/GetLabeledData/" + validDataPointIndex;
            var token = await GetAuthToken(new LoginModel { Email = Admin.Email, Password = Admin.Password });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task SetUpProjects()
        {
            // test admin project for text data points
            _adminProjectText = new Project("AdminProject", "My Description");
            _adminProjectText.Labels.Add(new Label("TestSet", "TestDesc"));
            var adminUser = await Context.Users.Where(u => u.Email == Admin.Email).FirstOrDefaultAsync();
            adminUser?.Projects.Add(_adminProjectText);
            await Context.AddAsync(_adminProjectText);
            await Context.SaveChangesAsync();

            // test admin project for image data points
            _adminProjectImage = new Project("AdminProject", "My Description", _testProjectDirectoryPath);
            _adminProjectImage.Labels.Add(new Label("TestSet", "TestDesc"));
            adminUser?.Projects.Add(_adminProjectImage);
            await Context.AddAsync(_adminProjectImage);
            await Context.SaveChangesAsync();

            var projectDirectoryPath = Path.Combine(_testProjectDirectoryPath, $"project_{_adminProjectImage.Id}");
            Directory.CreateDirectory(projectDirectoryPath);
            _adminProjectImage.StoragePath = projectDirectoryPath;
            Context.Update(_adminProjectImage);
            await Context.SaveChangesAsync();
        }

        private async Task CleanupProjects()
        {
            try { ReloadContext(); } catch { }
            var adminProjTxt = await Context.Projects.FindAsync(_adminProjectText.Id);
            if (adminProjTxt != null)
            {
                Context.Remove(adminProjTxt);
                await Context.SaveChangesAsync();
            }
            var adminProjImg = await Context.Projects.FindAsync(_adminProjectImage.Id);
            if (adminProjImg != null)
            {
                Directory.Delete(adminProjImg.StoragePath, true);
                Context.Remove(adminProjImg);
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
                _adminProjectText.DataPoints.Add(dataPoint!);
            }
            Context.Update(_adminProjectText!);
            await Context.SaveChangesAsync();
        }

        private async Task AddSomeImageDataPoints()
        {
            var filePath = Path.Combine(_testFilesPath, "jpg", "zip_example_JPG_100kB_6_files.zip");
            var imagePaths = await ProcessZipFile(filePath);
            int index = 0;
            foreach (var imagePath in imagePaths)
            {
                // create the image data point and add it to the project
                _adminProjectImage.DataPoints.Add(CreateImageDataPoint(imagePath, index));
                index++;
            }
            await Context.SaveChangesAsync();
        }

        private async Task<ICollection<string>> ProcessZipFile(string filePath)
        {
            ICollection<string> dataPoints = new List<string>();
            string tempDirPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDirPath);

            try
            {
                using (ZipArchive archive = new ZipArchive(File.OpenRead(filePath)))
                {
                    var index = 0;
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string imageName = $"image_{index}{Path.GetExtension(entry.Name)}";
                        string imagePath = Path.Combine(tempDirPath, imageName);
                        await Task.Run(() => entry.ExtractToFile(imagePath));

                        string newPath = Path.Combine(_adminProjectImage.StoragePath, imageName);
                        File.Move(imagePath, newPath);
                        dataPoints.Add(newPath);
                        index++;
                    }
                }
            }
            catch
            {
                foreach (string addedFile in dataPoints)
                {
                    File.Delete(addedFile);
                }
                Directory.Delete(tempDirPath, true);
                throw;
            }
            Directory.Delete(tempDirPath, true);
            return dataPoints;
        }

        private ImageDataPoint CreateImageDataPoint(string path, int index)
        {
            var dataPoint = new ImageDataPoint(path, index);
            Context.Add(dataPoint);
            return dataPoint;
        }

        private async Task ClearTextDataPoints()
        {
            var dataPoints = await Context.TextDataPoints.Where(e => e.ProjectId == _adminProjectText.Id).ToListAsync();
            if (dataPoints.Any())
            {
                foreach (var dataPoint in dataPoints)
                {
                    Context.Remove(dataPoint);
                }

                await Context.SaveChangesAsync();
            }
        }

        private async Task ClearImageDataPoints()
        {
            var imageDataPoints = await Context.ImageDataPoints.Where(e => e.ProjectId == _adminProjectImage.Id).ToListAsync();
            if (imageDataPoints.Any())
            {
                foreach (var imageDataPoint in imageDataPoints)
                {
                    System.IO.File.Delete(imageDataPoint.Path);
                    Context.Remove(imageDataPoint);
                }
                
                await Context.SaveChangesAsync();
            }
        }

        private async Task LabelDataPointAsync(TestUser user, int dataPointIdx, int labelId) 
        {
            var project = await GetFullAdminProjectAsync();
            var dataPointId = project.DataPoints.Where(e => e.DataPointIndex == dataPointIdx).First().Id;
            var labeledDataPoint = new LabeledDataPoint { LabelId = labelId, UserId = user.Id, DataPointId = dataPointId };
            await Context.LabeledDataPoints.AddAsync(labeledDataPoint);
            await Context.SaveChangesAsync();
            ReloadContext();
        }

        private async Task<Project> GetFullAdminProjectAsync() 
        {
            return await Context.Projects.Include(e => e.Labels).Include(e => e.DataPoints).FirstAsync(e => e.Id == _adminProjectText.Id);
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
