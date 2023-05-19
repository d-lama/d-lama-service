using d_lama_service.Models;
using d_lama_service.Models.UserModels;
using d_lama_service.Models.UserModels;
using d_lama_service.Repositories;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly string _pepper;
        private static readonly string Censored = "**********";
        public static readonly int Iteration = 3;

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
        [HttpPost("AuthToken")]
        public async Task<IActionResult> AuthToken([FromBody] LoginModel loginRequest)
        {
            try
            {
                var user = (await _unitOfWork.UserRepository.FindAsync(e => e.Email == loginRequest.Email)).First();
                if (user != null)
                {
                    var hash = PasswordHasher.ComputeHash(loginRequest.Password, user.PasswordSalt, _pepper, Iteration);
                    if (hash == user.PasswordHash)
                    {
                        var token = Tokenizer.CreateToken(user, _configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], _configuration["Jwt:Key"]);
                        return Ok(token);
                    }
                }
            }
            catch { }
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
            var hash = PasswordHasher.ComputeHash(registerRequest.Password,salt, _pepper, Iteration);
            var user = new User(registerRequest.Email, registerRequest.FirstName, registerRequest.LastName, hash, salt, registerRequest.BirthDate, registerRequest.IsAdmin);
            
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();

            var createdResource = new { id = user.Id };
            return CreatedAtAction(nameof(Get), createdResource, createdResource);
        }

        /// <summary>
        /// Gets the authenticated user.
        /// </summary>
        /// <returns> The user. </returns>
        [HttpGet("Me")]
        public async Task<IActionResult> GetMe() 
        {
            User user = await GetAuthenticatedUserAsync();
            user.PasswordSalt = Censored;
            user.PasswordHash = Censored;
            return Ok(user);
        }

        /// <summary>
        /// Gets a user by id.
        /// </summary>
        /// <returns> The user. </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            User? user = await _unitOfWork.UserRepository.GetAsync(id);
            if (user == null) 
            {
                return NotFound();
            }
            user.PasswordSalt = Censored;
            user.PasswordHash = Censored;
            return Ok(user);
        }

        /// <summary>
        /// Patches the authenticated user.
        /// </summary>
        /// <param name="modifiedUser"> The properties to update. </param>
        /// <returns> The updated user. </returns>
        [HttpPatch("Me")]
        public async Task<IActionResult> EditMe([FromBody] EditUserModel modifiedUser) 
        {
            User user = await GetAuthenticatedUserAsync();
            
            if (modifiedUser.Password != null)
            {
                var hash = PasswordHasher.ComputeHash(modifiedUser.Password, user.PasswordSalt, _pepper, Iteration);
                if (hash != user.PasswordHash) // change password
                {
                    user.PasswordSalt = PasswordHasher.GenerateSalt();
                    user.PasswordHash = PasswordHasher.ComputeHash(modifiedUser.Password, user.PasswordSalt, _pepper, Iteration);
                }
            }

            user.FirstName = modifiedUser.FirstName ?? user.FirstName;
            user.LastName = modifiedUser.LastName ?? user.LastName;
            user.BirthDate = modifiedUser.BirthDate ?? user.BirthDate;
            user.IsAdmin = modifiedUser.IsAdmin ?? user.IsAdmin;

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();

            return await GetMe();
        }

        /// <summary>
        /// Gets the user ranking.
        /// </summary>
        /// <returns> A ranking list on success, else en 400 error message. </returns>
        [HttpGet("Ranking")]
        public async Task<IActionResult> GetUserRanking() 
        {
            var dataPointsCount = (await _unitOfWork.DataPointRespitory.GetAllAsync()).Count();
            if (dataPointsCount == 0) 
            {
                return BadRequest("Currently there is no ranking available, as there are no Projects with datapoints.");
            }

            var users = await _unitOfWork.UserRepository.GetAllAsync();
            var ranking = new List<UserRankingModel>();
            var authUser = await GetAuthenticatedUserAsync();
            foreach (var user in users) 
            {                
                var labeledDataPointsCount = (await _unitOfWork.LabeledDataPointRepository.FindAsync(e => e.UserId == user.Id)).Count();
                var percentage = (float)labeledDataPointsCount / (float)dataPointsCount;
                ranking.Add(new UserRankingModel(user.Id, user.FirstName + " " + user.LastName, percentage));
            }

            var orderedRanking = ranking.OrderByDescending(e => e.Percentage).ToList();
            var authUserPosition = orderedRanking.FindIndex(e => e.Id == authUser.Id);
            var data = new { myPositionIndex = authUserPosition, ranking = MakeRankingAnonymous(orderedRanking, authUser.Id)};
            return Ok(data);
        }

        private List<UserRankingModel> MakeRankingAnonymous(List<UserRankingModel> userRankingModels, int authUserId) 
        {
            const string maskedUserPrefix = "Lama-";
            var counter = 1;
            foreach (var user in userRankingModels) 
            {
                if (user.Id != authUserId) 
                {
                    user.Name = maskedUserPrefix + counter;
                    counter++;
                }
            }
            return userRankingModels;
        }

        private async Task<User> GetAuthenticatedUserAsync() 
        {
            var userId = int.Parse(HttpContext.User.FindFirst(Tokenizer.UserIdClaim)?.Value!); // on error throw
            return (await _unitOfWork.UserRepository.GetAsync(userId))!;
        }
    }
}
