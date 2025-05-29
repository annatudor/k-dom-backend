using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Common;
using Spryer;

namespace KDomBackend.Models.DTOs.AuditLog
{
    public class AuditLogFilterDto : PagedFilterDto
    {
        public DbEnum<AuditAction>? Action { get; set; }
        public int? UserId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        

    }
}
