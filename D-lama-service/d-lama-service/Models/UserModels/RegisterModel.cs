using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models.UserModels
{
    public class RegisterModel : LoginModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public bool IsAdmin { get; set; }
    }
}
