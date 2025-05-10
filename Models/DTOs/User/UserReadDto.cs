namespace KDomBackend.Models.DTOs.User
{
    public class UserReadDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

}
