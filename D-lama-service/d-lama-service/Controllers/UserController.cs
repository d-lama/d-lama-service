using d_lama_service.Attributes;
using d_lama_service.Models;
using d_lama_service.Repositories;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace d_lama_service.Controllers
{
    /// <summary>
    /// The User Controller handles the user specific requests.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly int _iteration = 3;
        private readonly string _pepper;

        /// <summary>
        /// Constructor of the UserController.
        /// </summary>
        /// <param name="unitOfWork"> The unitOfWork for handling db access. </param>
        /// <param name="configuration"> The configuration. </param>
        public UserController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _pepper = Environment.GetEnvironmentVariable("ASPNETCORE_PEPPER")!;
        }

        /// <summary>
        /// Returns an JWT-Auth token if the provided credentials are correct.
        /// </summary>
        /// <param name="loginRequest"> The credentials of the user. </param>
        /// <returns> Status Code 200 with a token on sucess. </returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("AuthToken")]
        public async Task<IActionResult> AuthToken([FromBody] LoginModel loginRequest)
        {
            var user = (await _unitOfWork.UserRepository.FindAsync(e => e.Email == loginRequest.Email)).FirstOrDefault();
            if (user != null) 
            {
                var hash = PasswordHasher.ComputeHash(loginRequest.Password, user.PasswordSalt, _pepper, _iteration);
                if (hash == user.PasswordHash) 
                {
                    var token = Tokenizer.CreateToken(user, _configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], _configuration["Jwt:Key"]);
                    return Ok(token);
                }
            }
            return Unauthorized("Provided username and password did not match!");
        }

        /// <summary>
        /// Registers / creates a user. Checks if the provided email address already exists and fails if so. 
        /// If user is valid, it creates a salt and password hash of the user and saves it to the db.
        /// </summary>
        /// <param name="registerRequest"> The register request containing all needed information to register a user. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterModel registerRequest)
        {
            var mailExists = (await _unitOfWork.UserRepository.FindAsync(e => e.Email == registerRequest.Email)).Any();
            if (mailExists) 
            {
                return BadRequest("This email address is already used!");
            }

            var salt = PasswordHasher.GenerateSalt();
            var hash = PasswordHasher.ComputeHash(registerRequest.Password,salt, _pepper, _iteration);
            var user = new User(registerRequest.Email, registerRequest.FirstName, registerRequest.LastName, hash, salt, registerRequest.BirthDate, registerRequest.IsAdmin);
            
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        /// <summary>
        /// Gets the authenticated user.
        /// </summary>
        /// <returns> The user. </returns>
        [HttpGet]
        public async Task<IActionResult> Get() 
        {
            User user = await GetAuthenticatedUserAsync();
            user.PasswordSalt = "***";
            user.PasswordHash = "***";
            return Ok(user);
        }

        /// <summary>
        /// Patches the authenticated user.
        /// </summary>
        /// <param name="modifiedUser"> The properties to update. </param>
        /// <returns> The updated user. </returns>
        [HttpPatch]
        public async Task<IActionResult> Edit([FromBody] EditUserModel modifiedUser) 
        {
            User user = await GetAuthenticatedUserAsync();
            
            if (modifiedUser.Password != null)
            {
                var hash = PasswordHasher.ComputeHash(modifiedUser.Password, user.PasswordSalt, _pepper, _iteration);
                if (hash != user.PasswordHash) // change password
                {
                    user.PasswordSalt = PasswordHasher.GenerateSalt();
                    user.PasswordHash = PasswordHasher.ComputeHash(modifiedUser.Password, user.PasswordSalt, _pepper, _iteration);
                }
            }

            user.FirstName = modifiedUser.FirstName ?? user.FirstName;
            user.LastName = modifiedUser.LastName ?? user.LastName;
            user.BirthDate = modifiedUser.BirthDate ?? user.BirthDate;
            user.IsAdmin = modifiedUser.IsAdmin ?? user.IsAdmin;

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();

            return await Get();
        }

        [HttpGet]
        [Route("TestCheckAuth")]
        public async Task<IActionResult> CheckAuthentication()
        {
            return Ok("If you get this message, then you are authenticated!");
        }

        [AdminAuthorize]
        [HttpGet]
        [Route("TestCheckAdmin")]
        public async Task<IActionResult> CheckAdmin()
        {
            return Ok("If you get this message, then you have admin permissions!");
        }

        private async Task<User> GetAuthenticatedUserAsync() 
        {
            var userId = int.Parse(HttpContext.User.FindFirst(Tokenizer.UserIdClaim)?.Value!); // on error throw
            return (await _unitOfWork.UserRepository.GetAsync(userId))!;
        }
    }
}
