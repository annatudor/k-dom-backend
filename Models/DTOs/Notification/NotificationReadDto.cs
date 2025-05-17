using KDomBackend.Enums;

namespace KDomBackend.Models.DTOs.Notification
{
    public class NotificationReadDto
    {
        public string Id { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? TriggeredByUsername { get; set; } = null;
        public ContentType? TargetType { get; set; }
        public string? TargetId { get; set; }
    }
}
