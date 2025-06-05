using KDomBackend.Enums;

namespace KDomBackend.Models.DTOs.Moderation
{
    public class ModerationActionDto
    {
        public int Id { get; set; }
        public string KDomId { get; set; } = string.Empty;
        public string KDomTitle { get; set; } = string.Empty;
        public string ModeratorUsername { get; set; } = string.Empty;
        public ModerationDecision Decision { get; set; }
        public string? Reason { get; set; }
        public DateTime ActionDate { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
    }

    public enum ModerationDecision
    {
        Approved,
        Rejected,
        Pending
    }
}