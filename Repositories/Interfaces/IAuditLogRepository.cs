using KDomBackend.Enums;
using KDomBackend.Models.Entities;

namespace KDomBackend.Repositories.Interfaces
{
    public interface IAuditLogRepository
    {
        Task CreateAsync(AuditLog log);
        Task<List<AuditLog>> GetAllAsync();
        Task<AuditLog?> GetByKDomAndUserAsync(string kdomId, int userId, AuditAction action);
        Task<List<AuditLog>> GetModerationActionsAsync(int limit = 50);
        Task<List<AuditLog>> GetModerationActionsByModeratorAsync(int moderatorId, DateTime? fromDate = null);
        Task<List<AuditLog>> GetModerationActionsForKDomAsync(string kdomId);
        Task<Dictionary<int, int>> GetModeratorStatsAsync(DateTime fromDate);
        Task<AuditLog?> GetLastModerationActionAsync(string kdomId);

    }
}
