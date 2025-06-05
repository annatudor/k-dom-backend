using KDomBackend.Enums;

namespace KDomBackend.Models.DTOs.Moderation
{
    public class UserKDomStatusDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public KDomModerationStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? ModeratedAt { get; set; }
        public string? ModeratorUsername { get; set; }
        public TimeSpan? ProcessingTime { get; set; }
        public bool CanEdit { get; set; } = false; // Doar pentru approved
        public bool CanResubmit { get; set; } = false; // Pentru rejected
    }

    public enum KDomModerationStatus
    {
        Pending,
        Approved,
        Rejected,
        Deleted
    }
}