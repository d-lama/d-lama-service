using System.Security.Cryptography;
using System.Text;

namespace d_lama_service.Models
{
    /// <summary>
    /// The PasswordHasher is used to create salts and password hashes.
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Computes the hash for a password.
        /// </summary>
        /// <param name="password"> The password. </param>
        /// <param name="salt"> The to the password corresponding salt. </param>
        /// <param name="pepper"> The pepper of the system. </param>
        /// <param name="iteration"> The number of iterations. </param>
        /// <returns> The password hash. </returns>
        public static string ComputeHash(string password, string salt, string pepper, int iteration)
        {
            if (iteration <= 0) return password;

            using var sha256 = SHA256.Create();
            var passwordSaltPepper = $"{password}{salt}{pepper}";
            var byteValue = Encoding.UTF8.GetBytes(passwordSaltPepper);
            var byteHash = sha256.ComputeHash(byteValue);
            var hash = Convert.ToBase64String(byteHash);
            return ComputeHash(hash, salt, pepper, iteration - 1);
        }

        /// <summary>
        /// Creates a new random salt. 
        /// </summary>
        /// <returns> The salt. </returns>
        public static string GenerateSalt()
        {
            using var rng = RandomNumberGenerator.Create();
            var byteSalt = new byte[16];
            rng.GetBytes(byteSalt);
            var salt = Convert.ToBase64String(byteSalt);
            return salt;
        }
    }
}
