using KDomBackend.Models.Entities;

namespace KDomBackend.Repositories.Interfaces
{
    public interface IAuditLogRepository
    {
        Task CreateAsync(AuditLog log);
        Task<List<AuditLog>> GetAllAsync();

    }
}
