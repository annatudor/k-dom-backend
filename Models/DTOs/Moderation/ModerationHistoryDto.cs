namespace KDomBackend.Models.DTOs.Moderation
{
    public class ModerationHistoryDto
    {
        public List<UserKDomStatusDto> AllSubmissions { get; set; } = new();
        public ModerationStatsDto UserStats { get; set; } = new();
        public List<ModerationActionDto> RecentDecisions { get; set; } = new();
    }

    public class UserModerationStatsDto
    {
        public int TotalSubmitted { get; set; }
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }
        public int TotalPending { get; set; }
        public double ApprovalRate { get; set; }
        public TimeSpan AverageProcessingTime { get; set; }
        public DateTime? FirstSubmission { get; set; }
        public DateTime? LastSubmission { get; set; }
    }
}
