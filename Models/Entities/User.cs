namespace KDomBackend.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? Provider { get; set; }
        public string? ProviderId { get; set; }
        public int RoleId { get; set; }
        public string? Role { get; set; } // nu e in baza de date
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
