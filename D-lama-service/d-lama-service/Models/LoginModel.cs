using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(10)]
        public string Password { get; set; }
    }
}
