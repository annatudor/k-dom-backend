using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.User
{
    public class UserProfileUpdateDto
    {
        [MaxLength(50)]
        public string Nickname { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Bio { get; set; } = string.Empty;

        [Url]
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
