using System.ComponentModel.DataAnnotations;

namespace d_lama_service.Models
{
    public class EditUserModel
    {
        [MinLength(10)]
        public string? Password { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime? BirthDate { get; set; }

        public bool? IsAdmin { get; set; }
    }
}
