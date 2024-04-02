using System.ComponentModel.DataAnnotations;

namespace Besos.Models.DTOs
{
    public class UserRegistrationRequestDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
