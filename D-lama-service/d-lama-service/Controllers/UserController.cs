using d_lama_service.Middleware;
using d_lama_service.Models.UserModels;
using d_lama_service.Services;
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
        private readonly IUserService _userService;
        private readonly ISharedService _sharedService;

        /// <summary>
        /// Constructor of the UserController.
        /// </summary>
        /// <param name="userService"> The user service which handles the domain logic for the users. </param>
        /// <param name="sharedService"> The shared service which provides shared methods. </param>
        public UserController(IUserService userService, ISharedService sharedService)
        {
            _userService = userService;
            _sharedService = sharedService;
        }

        /// <summary>
        /// Returns an JWT-Auth token if the provided credentials are correct.
        /// </summary>
        /// <param name="loginRequest"> The credentials of the user. </param>
        /// <returns> Status Code 200 with a token on success. </returns>
        [AllowAnonymous]
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpPost("AuthToken")]
        public async Task<IActionResult> AuthToken([FromBody] LoginModel loginRequest)
        {
            string token = await _userService.GetAuthTokenAsync(loginRequest);
            return Ok(token);
        }

        /// <summary>
        /// Registers / creates a user. Checks if the provided email address already exists and fails if so. 
        /// If user is valid, it creates a salt and password hash of the user and saves it to the db.
        /// </summary>
        /// <param name="registerRequest"> The register request containing all needed information to register a user. </param>
        /// <returns> Statuscode 200 on success, else Statuscode 400. </returns>
        [AllowAnonymous]
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterModel registerRequest)
        {
            var userId = await _userService.CreateUserAsync(registerRequest);
            var createdResource = new { id = userId };
            return CreatedAtAction(nameof(Get), createdResource, createdResource);
        }

        /// <summary>
        /// Gets the authenticated user.
        /// </summary>
        /// <returns> The user. </returns>
        [HttpGet("Me")]
        public async Task<IActionResult> GetMe() 
        {
            User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
            return Ok(user);
        }

        /// <summary>
        /// Gets a user by id.
        /// </summary>
        /// <returns> The user. </returns>
        [TypeFilter(typeof(RESTExceptionFilter))]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _userService.GetUserAsync(id);
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
            User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
            await _userService.UpdateUserAsync(user, modifiedUser);
            return await GetMe();
        }

        /// <summary>
        /// Gets the user ranking.
        /// </summary>
        /// <returns> A ranking list on success, else en 400 error message. </returns>
        [HttpGet("Ranking")]
        public async Task<IActionResult> GetUserRanking() 
        {
            User user = await _sharedService.GetAuthenticatedUserAsync(HttpContext);
            UserRankingListModel rankingList = await _userService.GetUserRankingTableAsync(user);
            return Ok(rankingList);
        }
    }
}
