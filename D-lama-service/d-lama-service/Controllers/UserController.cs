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
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly int _iteration = 3;
        private readonly string _pepper;

        public UserController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _pepper = Environment.GetEnvironmentVariable("ASPNETCORE_PEPPER")!;
        }

        /// <summary>
        /// Logs a user in if the provided credentials are correct.
        /// </summary>
        /// <param name="loginRequest"> The credentials of the user. </param>
        /// <returns> Status Code 200 with a token on sucess. </returns>
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginRequest)
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
        /// Registers a user. Checks if the provided email address already exists and fails if so. 
        /// If user is valid, it creates a salt and password hash of the user and saves it to the db.
        /// </summary>
        /// <param name="registerRequest"> The register request containing all needed information to register a user. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerRequest)
        {
            var mailExists = (await _unitOfWork.UserRepository.FindAsync(e => e.Email == registerRequest.Email)).Any();
            if (mailExists) 
            {
                return BadRequest("This email address is alredy used!");
            }

            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            var salt = PasswordHasher.GenerateSalt();
            var hash = PasswordHasher.ComputeHash(registerRequest.Password,salt, _pepper, _iteration);
            var user = new User(registerRequest.Email, registerRequest.FirstName, registerRequest.LastName, hash, salt, registerRequest.BirthDate, registerRequest.IsAdmin);
            
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();

            return Ok();
        }


        [Authorize]
        [HttpGet]
        [Route("CheckAuth")]
        public async Task<IActionResult> CheckAuthentication()
        {
            return Ok("If you get this message, then everything worked!");
        }
    }
}
