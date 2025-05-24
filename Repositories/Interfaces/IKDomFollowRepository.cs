using KDomBackend.Models.MongoEntities.KDomBackend.Models.MongoEntities;

namespace KDomBackend.Repositories.Interfaces
{
    public interface IKDomFollowRepository
    {
        Task<bool> ExistsAsync(int userId, string kdomId);
        Task CreateAsync(KDomFollow follow);
        Task UnfollowAsync(int userId, string kdomId);
        Task<List<string>> GetFollowedKDomIdsAsync(int userId);
        Task<Dictionary<string, int>> CountRecentFollowsAsync(int days = 7);
        Task<List<string>> GetFollowedSlugsAsync(int userId);
        Task<int> CountFollowersAsync(string kdomId);

    }
}
