using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d_lama_service.Controllers;
using d_lama_service.Models;
using Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace MSTest.IntegrationTests
{
    public class APITestBase
    {
        protected DataContext Context;
        private readonly IConfiguration _configuration;
        public readonly TestUser User = new TestUser("test@gmail.com", "123MySecurePW!?", false);
        public readonly TestUser Admin = new TestUser("admin@gmail.com", "123MySecurePW!?", true);
        public readonly List<TestUser> TestUsers = new List<TestUser>();

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
            TestUsers.Add(User);
        }

        public void ReloadContext() 
        {
            // initialize Context
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("tst"));
            Context = new DataContext(optionsBuilder.Options);
        }
                
        /// <summary>
        /// Before each integration test.
        /// Sets up test users.
        /// </summary>
        /// <returns></returns>
        [TestInitialize]
        public async Task Initialize()
        {
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
        }

        private async Task SetUpUsers()
        {
            var pepper = Environment.GetEnvironmentVariable("ASPNETCORE_PEPPER")!;
            foreach (var testUser in TestUsers)
            {
                var salt = PasswordHasher.GenerateSalt();
                var hash = PasswordHasher.ComputeHash(testUser.Password, salt, pepper, UserController.Iteration);
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

            public TestUser(string email, string password, bool isAdmin)
            {
                Email = email;
                Password = password;
                IsAdmin = isAdmin;
            }
        }
    }
}
