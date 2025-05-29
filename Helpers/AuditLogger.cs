using KDomBackend.Enums;
using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;

namespace KDomBackend.Helpers
{
    public static class AuditLogger
    {
        public static async Task LogAsync(
            IAuditLogRepository repository,
            int? userId,
            AuditAction action,
            AuditTargetType targetType,
            string? targetId,
            string details)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                TargetType = targetType,
                TargetId = targetId,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await repository.CreateAsync(log);
        }
    }
}