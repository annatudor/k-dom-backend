using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Common;

namespace KDomBackend.Models.DTOs.AuditLog
{
    public class AuditLogFilterDto : PagedFilterDto
    {
        public AuditAction? Action { get; set; }
        public int? UserId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        

    }
}
