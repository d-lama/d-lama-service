using System.Text;
using d_lama_service.Models;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using d_lama_service;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using d_lama_service.Models.UserModels;
using Data.ProjectEntities;
using d_lama_service.Services;

namespace Test.IntegrationTests
{
    public class APITestBase
    {
        protected readonly TestUser User = new TestUser("test@gmail.com", "123MySecurePW!?", false);
        protected readonly TestUser Admin = new TestUser("admin@gmail.com", "123MySecurePW!?", true);
        protected readonly TestUser Admin2 = new TestUser("admin2@gmail.com", "123MySecurePW!?", true);
        protected readonly List<TestUser> TestUsers = new List<TestUser>();
        protected HttpClient Client;
        protected DataContext Context;
        private readonly IConfiguration _configuration;
        private WebApplicationFactory<Startup> _factory;

        /// <summary>
        /// Constructor of IntegrationTestBase.
        /// Loads configuration defined in appsettings.json.
        /// Defines test database through ConnectionString identifier 'tst'.
        /// </summary>
        public APITestBase() 
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile(@"appsettings.json")
                .AddEnvironmentVariables()
                .Build(); // initialize config

            ReloadContext();

            TestUsers.Add(Admin);
            TestUsers.Add(Admin2);
            TestUsers.Add(User);
        }

        protected void ReloadContext() 
        {
            // initialize Context
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("tst"));
            Context = new DataContext(optionsBuilder.Options);
        }

        protected async Task<string> GetAuthToken(LoginModel login)
        {
            var uri = "/api/User/AuthToken";
            var content = new StringContent(JsonConvert.SerializeObject(login), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;
            var response = await Client.SendAsync(request);
            return response.Content.ReadAsStringAsync().Result;
        }

        /// <summary>
        /// Before each integration test.
        /// Sets up HttpClient for accessing REST-API.
        /// Sets up test users.
        /// </summary>
        /// <returns></returns>
        [TestInitialize]
        public async Task Initialize()
        {
            _factory = new WebApplicationFactory<Startup>();
            Client = _factory.CreateClient();
            await SetUpUsers();
        }

        /// <summary>
        /// After each integration test.
        /// Removes test users.
        /// </summary>
        /// <returns></returns>
        [TestCleanup]
        public async Task Cleanup() 
        {
            await CleanupUsers();
            _factory.Dispose();
        }

        private async Task SetUpUsers()
        {
            var pepper = Environment.GetEnvironmentVariable("ASPNETCORE_PEPPER")!;
            foreach (var testUser in TestUsers)
            {
                var salt = PasswordHasher.GenerateSalt();
                var hash = PasswordHasher.ComputeHash(testUser.Password, salt, pepper, UserService.Iteration);
                var user = new User(testUser.Email, "my first name", "my last name", hash, salt, DateTime.Today.AddYears(-20), testUser.IsAdmin);
                await Context.AddAsync(user);
                await Context.SaveChangesAsync();
                testUser.Id = user.Id;
            }
        }

        private async Task CleanupUsers()
        {
            foreach (var testUser in TestUsers)
            {
                var user = await Context.Users.Where(e => e.Email == testUser.Email).FirstAsync();
                if (user != null)
                {
                    Context.Remove(user);
                }
            }
            await Context.SaveChangesAsync();
        }

        public class TestUser
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public bool IsAdmin { get; set; }   
            public int Id { get; set; }

            // Collection navigation containing dependents
            public ICollection<Project> Projects { get; } = new List<Project>();
            public ICollection<TextDataPoint> TextDataPoints { get; } = new List<TextDataPoint>();

            public TestUser(string email, string password, bool isAdmin)
            {
                Email = email;
                Password = password;
                IsAdmin = isAdmin;
            }
        }
    }
}
