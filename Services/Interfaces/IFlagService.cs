using KDomBackend.Models.DTOs.Flag;

namespace KDomBackend.Services.Interfaces
{
    public interface IFlagService
    {
        Task CreateFlagAsync(int userId, FlagCreateDto dto);
        Task<List<FlagReadDto>> GetAllAsync();
        Task ResolveAsync(int flagId);
        Task DeleteAsync(int flagId);

    }
}
