using KDomBackend.Models.DTOs.Common;

namespace KDomBackend.Models.DTOs.User
{
    public class UserPublicDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

}
