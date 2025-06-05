using KDomBackend.Enums;

namespace KDomBackend.Models.DTOs.Moderation
{
    public class KDomModerationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContentHtml { get; set; } = string.Empty;
        public Hub Hub { get; set; }
        public Language Language { get; set; }
        public bool IsForKids { get; set; }
        public KDomTheme Theme { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ParentId { get; set; }
        public string? ParentTitle { get; set; }
        public TimeSpan WaitingTime => DateTime.UtcNow - CreatedAt;
        public ModerationPriority Priority { get; set; } = ModerationPriority.Normal;
        public List<string> Flags { get; set; } = new(); // Pentru viitoarele flag-uri
    }

    public enum ModerationPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }
}