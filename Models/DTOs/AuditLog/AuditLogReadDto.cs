using KDomBackend.Enums;
using Spryer;

namespace KDomBackend.Models.DTOs.Audit
{
    public class AuditLogReadDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public DbEnum<AuditAction> Action { get; set; }
        public DbEnum<AuditTargetType> TargetType { get; set; }
        public string? TargetId { get; set; } = string.Empty;
        public string? Details { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
