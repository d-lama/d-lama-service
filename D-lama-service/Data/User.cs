using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    /// <summary>
    /// User entity.
    /// </summary>
    public class User : Entity
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string PasswordSalt { get; set; } 

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public bool IsAdmin { get; set; }

        public User(string email, string firstName, string lastName, string passwordHash, string passwordSalt, DateTime birthDate, bool isAdmin)
        {
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            BirthDate = birthDate.Date;
            IsAdmin = isAdmin;
        }
    }
}
