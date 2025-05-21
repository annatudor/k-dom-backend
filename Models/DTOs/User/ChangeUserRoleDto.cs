using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.User
{
    public class ChangeUserRoleDto
    {
        [Required]
        [RegularExpression("^(user|moderator|admin)$", ErrorMessage = "Invalid role.")]
        public string NewRole { get; set; } = string.Empty;
    }
}
