using KDomBackend.Models.DTOs.Audit;
using KDomBackend.Models.DTOs.AuditLog;
using KDomBackend.Models.DTOs.Common;

namespace KDomBackend.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<List<AuditLogReadDto>> GetAllAsync();
        Task<PagedResult<AuditLogReadDto>> GetFilteredAsync(AuditLogFilterDto filters);


    }
}
