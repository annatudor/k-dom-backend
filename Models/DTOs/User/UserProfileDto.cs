namespace KDomBackend.Models.DTOs.User
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;

        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}
