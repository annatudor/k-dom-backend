namespace KDomBackend.Models.DTOs.User
{
    public class UserDetailedStatsDto
    {
        public int TotalKDomEdits { get; set; }
        public int TotalLikesReceived { get; set; }
        public int TotalLikesGiven { get; set; }
        public int TotalCommentsReceived { get; set; }
        public int TotalFlagsReceived { get; set; }
        public Dictionary<string, int> ActivityByMonth { get; set; } = new();
        public List<string> RecentActions { get; set; } = new(); // Ultimele 10 acțiuni
    }
}
