using KDomBackend.Enums;

namespace KDomBackend.Models.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;

        public int UserId { get; set; }

        public AuditTargetType? TargetType { get; set; } 
        public string? TargetId { get; set; }            

        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
