namespace KDomBackend.Models.DTOs.Moderation
{
    public class ModerationDashboardDto
    {
        public ModerationStatsDto Stats { get; set; } = new();
        public List<KDomModerationDto> PendingKDoms { get; set; } = new();
        public List<ModerationActionDto> RecentActions { get; set; } = new();
    }

    public class ModerationStatsDto
    {
        public int TotalPending { get; set; }
        public int TotalApprovedToday { get; set; }
        public int TotalRejectedToday { get; set; }
        public int TotalApprovedThisWeek { get; set; }
        public int TotalRejectedThisWeek { get; set; }
        public int TotalApprovedThisMonth { get; set; }
        public int TotalRejectedThisMonth { get; set; }
        public double AverageProcessingTimeHours { get; set; }
        public List<ModeratorActivityDto> ModeratorActivity { get; set; } = new();
    }

    public class ModeratorActivityDto
    {
        public string ModeratorUsername { get; set; } = string.Empty;
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalActions { get; set; }
    }
}