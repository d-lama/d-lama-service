using d_lama_service.Models;
using d_lama_service.Models.UserModels;
using d_lama_service.Repositories;
using d_lama_service.Repositories.ProjectRepositories;
using Data;
using System.Net;

namespace d_lama_service.Services
{
    /// <summary>
    /// The User Service handles the domain specific logic regarding the users.
    /// </summary>
    public class UserService : Service, IUserService
    {
        private IUserRepository _userRepository;
        private IDataPointRepository _dataPointRepository;
        private ILabeledDataPointRepository _labeledDataPointRepository;
        private readonly IConfiguration _configuration;
        private readonly string _pepper;
        public static readonly int Iteration = 3;

        /// <summary>
        /// Constructor of the UserService.
        /// </summary>
        /// <param name="context"> The database context. </param>
        /// <param name="configuration"> The configuration. </param>
        public UserService(DataContext context, IConfiguration configuration) : base(context)
        {
            _userRepository = new UserRepository(context);
            _dataPointRepository = new DataPointRepository(context);
            _labeledDataPointRepository = new LabeledDataPointRepository(context);
            _configuration = configuration;
            _pepper = Environment.GetEnvironmentVariable("ASPNETCORE_PEPPER")!;
        }

        public async Task<int> CreateUserAsync(RegisterModel registerRequest) 
        {
            var mailExists = (await _userRepository.FindAsync(e => e.Email == registerRequest.Email)).Any();
            if (mailExists) 
            {
                throw new RESTException(HttpStatusCode.BadRequest, "This email address is already used!");
            }

            var salt = PasswordHasher.GenerateSalt();
            var hash = PasswordHasher.ComputeHash(registerRequest.Password, salt, _pepper, Iteration);
            var user = new User(registerRequest.Email, registerRequest.FirstName, registerRequest.LastName, hash, salt, registerRequest.BirthDate, registerRequest.IsAdmin);

            _userRepository.Update(user);
            await SaveAsync();
            return user.Id;
        }

        public async Task<User> GetUserAsync(int id) 
        {
            User? user = await _userRepository.GetAsync(id);
            if (user == null) 
            {
                throw new RESTException(HttpStatusCode.NotFound, string.Empty);
            }
            return user;
        }

        public async Task UpdateUserAsync(User user, EditUserModel modifiedUser) 
        {
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

            _userRepository.Update(user);
            await SaveAsync();
        }

        public async Task<string> GetAuthTokenAsync(LoginModel loginRequest) 
        {
            var user = (await _userRepository.FindAsync(e => e.Email == loginRequest.Email)).FirstOrDefault();
            if (user != null)
            {
                var hash = PasswordHasher.ComputeHash(loginRequest.Password, user.PasswordSalt, _pepper, Iteration);
                if (hash == user.PasswordHash)
                {
                    var token = Tokenizer.CreateToken(user, _configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], _configuration["Jwt:Key"]);
                    return token;
                }
                throw new RESTException(HttpStatusCode.Unauthorized, "Passwords did not match!");
            }
            throw new RESTException(HttpStatusCode.Unauthorized, "Username is invalid!");
        }


        public async Task<UserRankingListModel> GetUserRankingTableAsync(User authUser) 
        {
            var dataPointsCount = (await _dataPointRepository.GetAllAsync()).Count();
            if (dataPointsCount == 0)
            {
                throw new RESTException(HttpStatusCode.BadRequest,"Currently there is no ranking available, as there are no Projects with datapoints.");
            }

            var users = await _userRepository.GetAllAsync();
            var ranking = new List<UserRankingModel>();
            foreach (var user in users)
            {
                var labeledDataPointsCount = (await _labeledDataPointRepository.FindAsync(e => e.UserId == user.Id)).Count();
                var percentage = (float)labeledDataPointsCount / (float)dataPointsCount;
                ranking.Add(new UserRankingModel(user.Id, user.FirstName + " " + user.LastName, percentage));
            }

            var orderedRanking = ranking.OrderByDescending(e => e.Percentage).ToList();
            var authUserPosition = orderedRanking.FindIndex(e => e.Id == authUser.Id);
            var anonymousRanking = MakeRankingAnonymous(orderedRanking, authUser.Id);
            return new UserRankingListModel(authUserPosition, anonymousRanking);
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
    }
}
