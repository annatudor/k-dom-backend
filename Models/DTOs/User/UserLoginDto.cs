using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.User
{
    public class UserLoginDto
    {
        [Required]
        public string Identifier { get; set; } = string.Empty; // email sau username

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
