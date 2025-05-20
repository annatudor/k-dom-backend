using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Models.DTOs.Flag;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class FlagService : IFlagService
    {
        private readonly IFlagRepository _repository;

        public FlagService(IFlagRepository repository)
        {
            _repository = repository;
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

        public async Task ResolveAsync(int flagId)
        {
            await _repository.MarkResolvedAsync(flagId);
        }

        public async Task DeleteAsync(int flagId)
        {
            await _repository.DeleteAsync(flagId);
        }

    }
}
