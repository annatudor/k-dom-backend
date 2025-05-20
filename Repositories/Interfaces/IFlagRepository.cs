using KDomBackend.Models.Entities;

namespace KDomBackend.Repositories.Interfaces
{
    public interface IFlagRepository
    {
        Task CreateAsync(Flag flag);
        Task<List<Flag>> GetAllAsync();
        Task MarkResolvedAsync(int id);
        Task DeleteAsync(int id);

    }
}
