using d_lama_service;
using d_lama_service.Models;
using Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MSTest.IntegrationTests
{
    [TestClass]
    public class UserControllerTest : IntegrationTestBase
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private readonly string _apiRoute = "api/User";

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
        public void AfterEach()
        {
            _factory.Dispose();
        }

        /// <summary>
        /// Tests the creation of a user.
        /// 
        /// API PATH: /api/User
        /// METHOD: POST
        /// 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CreateUserTest() 
        {
            // # 1. Invalid request object.
            // Arrange
            var requestObj = new { email = "", password = "", firstname = "", lastname = "", birthdate = "", isAdmin = false};
            var content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiRoute);
            request.Content = content;
            // Act
            var response = await _client.SendAsync(request);
            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            // # 2. Valid request object.
            // Arrange
            var mailAddress = GenerateNewMail();
            requestObj = new { email = mailAddress, password = "mylongpassword", firstname = "myfirstName", lastname = "test", birthdate = "2023-04-04T07:44:51.945Z", isAdmin = false };
            content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            request = new HttpRequestMessage(HttpMethod.Post, _apiRoute);
            request.Content = content;
            // Act
            response = await _client.SendAsync(request);
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // # 3. Use same email address again
            // Arrange
            requestObj = new { email = mailAddress, password = "asdadasdasdasda", firstname = "myLastName", lastname = "test", birthdate = "2023-04-04T07:44:51.945Z", isAdmin = false };
            content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            request = new HttpRequestMessage(HttpMethod.Post, _apiRoute);
            request.Content = content;
            // Act
            response = await _client.SendAsync(request);
            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Tests the auth token (login) request.
        /// 
        /// API PATH: /api/User/AuthToken
        /// METHOD: POST
        /// 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AuthTokenTest()
        {
            // # 1. Missing credentials
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

            // # 2. Wrong credentials
            // Arrange
            requestObj = new LoginModel { Email = "wrongMail@test.com", Password = "WrongPassword" };
            content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;
            // Act
            response = await _client.SendAsync(request);
            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            // # 4. Wrong password, correct email
            // Arrange
            requestObj = new LoginModel { Email = User.Email, Password = "WrongPassword!!" };
            content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;
            // Act
            response = await _client.SendAsync(request);
            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            // # 3. Valid credentials
            // Arrange
            requestObj = new LoginModel { Email = User.Email, Password = User.Password };
            content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");
            request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;
            // Act
            response = await _client.SendAsync(request);
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Tests the authorization.
        /// 
        /// API PATH: /api/User
        /// METHOD: GET
        /// 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AuthTokenAuthorizationTest() 
        {
            // # 1. No Auth token provided
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, _apiRoute);
            // Act
            var response = await _client.SendAsync(request);
            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            // # 2. Invalid Auth token provided
            // Arrange
            request = new HttpRequestMessage(HttpMethod.Get, _apiRoute);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "InvalidToken");
            // Act
            response = await _client.SendAsync(request);
            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            // # 3. Valid Auth token provided
            // Arrange
            var token = await GetAuthToken(new LoginModel { Email = User.Email, Password = User.Password});
            request = new HttpRequestMessage(HttpMethod.Get, _apiRoute);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Act
            response = await _client.SendAsync(request);
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


        private string GenerateNewMail()
        {
            var mailSuffix = "@gmail.com";
            string mail;
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            while (true)
            {
                mail = new string(Enumerable.Repeat(chars, 20).Select(s => s[random.Next(s.Length)]).ToArray()) + mailSuffix;
                if (!Context.Users.Where(e => e.Email == mail).Any())
                {
                    return mail;
                }
            }
        }
    }
}
