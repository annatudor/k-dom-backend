using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Models.DTOs.Flag;
using KDomBackend.Services.Interfaces;
using KDomBackend.Enums;
using KDomBackend.Repositories.Implementations;

namespace KDomBackend.Services.Implementations
{
    public class FlagService : IFlagService
    {
        private readonly IFlagRepository _repository;
        private readonly IAuditLogRepository _auditLogRepository;

        public FlagService(IFlagRepository repository, IAuditLogRepository auditLogRepository)
        {
            _repository = repository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task CreateFlagAsync(int userId, FlagCreateDto dto)
        {
            var flag = new Flag
            {
                UserId = userId,
                ContentType = dto.ContentType,
                ContentId = dto.ContentId,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(flag);
        }

        public async Task<List<FlagReadDto>> GetAllAsync()
        {
            var flags = await _repository.GetAllAsync();

            return flags.Select(f => new FlagReadDto
            {
                Id = f.Id,
                UserId = f.UserId,
                ContentType = f.ContentType,
                ContentId = f.ContentId,
                Reason = f.Reason,
                CreatedAt = f.CreatedAt,
                IsResolved = f.IsResolved
            }).ToList();
        }

        public async Task ResolveAsync(int flagId, int userId)
        {
            await _repository.MarkResolvedAsync(flagId);
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                Action = AuditAction.ResolveFlag,
                TargetType = AuditTargetType.Flag,
                TargetId = flagId.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task DeleteAsync(int flagId, int userId)
        {
            await _repository.DeleteAsync(flagId);
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                Action = AuditAction.DeleteFlag,
                TargetType = AuditTargetType.Flag,
                TargetId = flagId.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }


    }
}
