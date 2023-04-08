﻿using d_lama_service;
using d_lama_service.Models.UserViewModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace MSTest.IntegrationTests
{
    [TestClass]
    public class UserControllerTest : APITestBase
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private readonly string _apiRoute = "api/User";
        private string _testEmailAddress = "this_is_a_test@gmail.com";

        /// <summary>
        /// Before each run.
        /// </summary>
        [TestInitialize]
        public void BeforeEach()
        {
            _factory = new WebApplicationFactory<Startup>();
            _client = _factory.CreateClient();  
        }

        /// <summary>
        /// After each run.
        /// </summary>
        [TestCleanup]
        public async Task AfterEach()
        {
            await CleanUpTestUser();
            _factory.Dispose();
        }

        [TestMethod]
        public async Task CreateUser_InvalidRequest() 
        {
            // Arrange
            var requestObj = new { email = "", password = "", firstname = "", lastname = "", birthdate = "", isAdmin = false };
            var content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiRoute);
            request.Content = content;
            
            // Act
            var response = await _client.SendAsync(request);
            
            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [TestMethod]
        public async Task CreateUser_ValidRequest()
        {
            // Arrange
            var requestObj = new { email = _testEmailAddress, password = "mylongpassword", firstname = "myfirstName", lastname = "test", birthdate = "2023-04-04T07:44:51.945Z", isAdmin = false };
            var content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, _apiRoute);
            request.Content = content;
            
            // Act
            var response = await _client.SendAsync(request);
            
            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateUser_TwiceSameMail()
        {
            // Arrange
            var requestObj = new { email = _testEmailAddress, password = "asdadasdasdasda", firstname = "myLastName", lastname = "test", birthdate = "2023-04-04T07:44:51.945Z", isAdmin = false };
            var content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, _apiRoute);
            request.Content = content;

            var content2 = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json"); // same content as before
            var request2 = new HttpRequestMessage(HttpMethod.Post, _apiRoute);
            request2.Content = content2;

            // Act
            var response = await _client.SendAsync(request);
            var response2 = await _client.SendAsync(request2);
            
            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(HttpStatusCode.BadRequest, response2.StatusCode);
        }



        [TestMethod]
        public async Task AuthToken_InvalidRequest() 
        {
            // Arrange
            var uri = _apiRoute + "/AuthToken";
            var requestObj = new LoginModel { Password = "OnlyPasswordProvided" };
            var content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task AuthToken_WrongCredentials_Email()
        {
            // Arrange
            var uri = _apiRoute + "/AuthToken";
            var requestObj = new LoginModel { Email = "wrongMail@test.com", Password = "WrongPassword" };
            var content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task AuthToken_WrongCredentials_Password()
        {
            // Arrange
            var uri = _apiRoute + "/AuthToken";
            var requestObj = new LoginModel { Email = User.Email, Password = "WrongPassword!!" };
            var content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task AuthToken_CorrectCredentials()
        {
            // Arrange
            var uri = _apiRoute + "/AuthToken";
            var requestObj = new LoginModel { Email = User.Email, Password = User.Password };
            var content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task AuthToken_Authorization_NoAuthToken() 
        {
            // Arrange
            var uri = _apiRoute + "/Me";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task AuthToken_Authorization_InvalidAuthToken()
        {
            // Arrange
            var uri = _apiRoute + "/Me";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "InvalidToken");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task AuthToken_Authorization_ValidAuthToken()
        {
            // Arrange
            var uri = _apiRoute + "/Me";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task EditMe_UnknownProperty()
        {
            // Arrange
            var uri = _apiRoute + "/Me";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(new { myProp = ""}), Encoding.UTF8, "application/json");
            request.Content = content;
            var user = Context.Users.Where(e => e.Email == User.Email).First();

            // Act
            var response = await _client.SendAsync(request);
            ReloadContext(); // context must be reloaded as there were changes
            var updatedUser = Context.Users.Where(e => e.Email == User.Email).First();

            // Assert - Nothing changed
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(user.Id, updatedUser.Id);
            Assert.AreEqual(user.Email, updatedUser.Email);
            Assert.AreEqual(user.FirstName, updatedUser.FirstName);
            Assert.AreEqual(user.LastName, updatedUser.LastName);
            Assert.AreEqual(user.IsAdmin, updatedUser.IsAdmin);
            Assert.AreEqual(user.PasswordHash, updatedUser.PasswordHash);
            Assert.AreEqual(user.PasswordSalt, updatedUser.PasswordSalt);
        }

        [TestMethod]
        public async Task EditMe_InvalidPassword()
        {
            // Arrange
            var uri = _apiRoute + "/Me";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(new EditUserModel{ Password = "toShort" }), Encoding.UTF8, "application/json");
            request.Content = content;

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task EditMe_ValidPassword()
        {
            // Arrange
            var uri = _apiRoute + "/Me";
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var request = new HttpRequestMessage(HttpMethod.Patch, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(JsonConvert.SerializeObject(new EditUserModel { Password = "MyNewPassword" }), Encoding.UTF8, "application/json");
            request.Content = content;
            var user = Context.Users.Where(e => e.Email == User.Email).First();

            // Act
            var response = await _client.SendAsync(request);
            ReloadContext(); // context must be reloaded as there were changes
            var updatedUser = Context.Users.Where(e => e.Email == User.Email).First();

            // Assert - Password Changed
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(user.Id, updatedUser.Id);
            Assert.AreEqual(user.Email, updatedUser.Email);
            Assert.AreEqual(user.FirstName, updatedUser.FirstName);
            Assert.AreEqual(user.LastName, updatedUser.LastName);
            Assert.AreEqual(user.IsAdmin, updatedUser.IsAdmin);
            Assert.AreNotEqual(user.PasswordHash, updatedUser.PasswordHash);
            Assert.AreNotEqual(user.PasswordSalt, updatedUser.PasswordSalt);
        }

        [TestMethod]
        public async Task Get_InvalidId()
        {
            // Arrange
            var id = "/-1";
            var uri = _apiRoute + id;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_ValidId()
        {
            // Arrange
            var id = "/" + User.Id;
            var uri = _apiRoute + id;
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password });
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task<string> GetAuthToken(LoginModel login) 
        {
            var uri = _apiRoute + "/AuthToken";
            var content = new StringContent(JsonConvert.SerializeObject(login), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;
            var response = await _client.SendAsync(request);
            return response.Content.ReadAsStringAsync().Result;
        }

        private async Task CleanUpTestUser()
        {
            var users = Context.Users.Where(e => e.Email == _testEmailAddress);
            if (users.Any())
            {
                var user = users.First();
                Context.Remove(user);
                await Context.SaveChangesAsync();
            }
        }
    }
}
