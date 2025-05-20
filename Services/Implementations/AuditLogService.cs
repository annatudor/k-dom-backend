using KDomBackend.Models.DTOs.Audit;
using KDomBackend.Models.DTOs.AuditLog;
using KDomBackend.Models.DTOs.Common;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;

        public AuditLogService(IAuditLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<AuditLogReadDto>> GetAllAsync()
        {
            var logs = await _repository.GetAllAsync();

            return logs.Select(log => new AuditLogReadDto
            {
                Id = log.Id,
                UserId = log.UserId,
                Action = log.Action,
                TargetType = log.TargetType,
                TargetId = log.TargetId,
                Details = log.Details,
                CreatedAt = log.CreatedAt
            }).ToList();
        }

        public async Task<PagedResult<AuditLogReadDto>> GetFilteredAsync(AuditLogFilterDto filters)
        {
            var all = await _repository.GetAllAsync();
            var query = all.AsQueryable();

            if (filters.Action.HasValue)
                query = query.Where(a => a.Action == filters.Action.Value);

            if (filters.UserId.HasValue)
                query = query.Where(a => a.UserId == filters.UserId.Value);

            if (filters.From.HasValue)
                query = query.Where(a => a.CreatedAt >= filters.From.Value);

            if (filters.To.HasValue)
                query = query.Where(a => a.CreatedAt <= filters.To.Value);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filters.PageSize);
            var skip = (filters.Page - 1) * filters.PageSize;

            var items = query
                .OrderByDescending(a => a.CreatedAt)
                .Skip(skip)
                .Take(filters.PageSize)
                .Select(a => new AuditLogReadDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    Action = a.Action,
                    TargetType = a.TargetType,
                    TargetId = a.TargetId,
                    Details = a.Details,
                    CreatedAt = a.CreatedAt
                })
                .ToList();

            return new PagedResult<AuditLogReadDto>
            {
                TotalCount = totalCount,
                PageSize = filters.PageSize,
                CurrentPage = filters.Page,
                TotalPages = totalPages,
                Items = items
            };
        }

    }
}
