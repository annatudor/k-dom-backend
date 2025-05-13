using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.User
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
