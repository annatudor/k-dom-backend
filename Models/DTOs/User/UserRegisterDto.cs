using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.User
{
    public class UserRegisterDto
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
    ErrorMessage = "The password must contain at least one uppercase letter, one lowercase letter, one number and one symbol.")]
        public string Password { get; set; } = string.Empty;
    }

}
