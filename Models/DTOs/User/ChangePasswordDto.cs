using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.User
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
        [MinLength(1, ErrorMessage = "Current password cannot be empty")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long")]
        public string NewPassword { get; set; } = string.Empty;
    }

}
