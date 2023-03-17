using Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace d_lama_service.Models
{
    /// <summary>
    /// The Tokenizer class creates JWT-Tokens which are used for authentication.
    /// </summary>
    public static class Tokenizer
    {
        public static readonly string UserIdClaim = "UserId";
        private static readonly int _tokeLifeTimeDays = 2;

        /// <summary>
        /// Creates a JWT-Stringtoken for a user.
        /// The token saves information like: UserId, Name, Email and JTI.
        /// </summary>
        /// <param name="user"> The user which the token is for. </param>
        /// <param name="issuer"> The issuer of the token. </param>
        /// <param name="audience"> The audience of the token. </param>
        /// <param name="key"> The key of the token. </param>
        /// <returns> A JWT-Token containing the most important user related information. </returns>
        public static string CreateToken(User user, string issuer, string audience, string key)
        {

            var keyBytes = Encoding.ASCII.GetBytes(key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                            new Claim(UserIdClaim, user.Id.ToString()),
                            new Claim(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
                            new Claim(JwtRegisteredClaimNames.Email, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                }),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(_tokeLifeTimeDays),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = tokenHandler.WriteToken(token);
            return stringToken;
        }

    }
}
