namespace KDomBackend.Models.DTOs.User
{
    public class UserPrivateInfoDto
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? Provider { get; set; } // Google, local, etc.
        public DateTime? LastLoginAt { get; set; }
    }
}
