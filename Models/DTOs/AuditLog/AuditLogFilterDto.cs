using KDomBackend.Enums;

namespace KDomBackend.Models.DTOs.AuditLog
{
    public class AuditLogFilterDto
    {
        public AuditAction? Action { get; set; }
        public int? UserId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

    }
}
