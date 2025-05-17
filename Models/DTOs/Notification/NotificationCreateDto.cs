using KDomBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.Notification
{
    public class NotificationCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public int? TriggeredByUserId { get; set; }

        public ContentType? TargetType { get; set; }

        public string? TargetId { get; set; }
    }
}
