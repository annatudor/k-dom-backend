using KDomBackend.Models.DTOs.Flag;
using KDomBackend.Enums;
using System.Threading.Tasks;

namespace KDomBackend.Services.Interfaces
{
    public interface IFlagService
    {
        Task CreateFlagAsync(int userId, FlagCreateDto dto);
        Task<List<FlagReadDto>> GetAllAsync();
        Task ResolveAsync(int flagId, int userId);
        Task DeleteAsync(int flagId, int userId);
        Task DeleteFlaggedContentAsync(int flagId, int moderatorId, string? moderationReason = null);

    }
}
