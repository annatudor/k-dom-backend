namespace KDomBackend.Repositories.Interfaces
{
    public interface IFollowRepository
    {
        Task<bool> ExistsAsync(int followerId, int followingId);
        Task CreateAsync(int followerId, int followingId);
        Task DeleteAsync(int followerId, int followingId);
        Task<List<int>> GetFollowersAsync(int userId); // followers ai lui X
        Task<List<int>> GetFollowingAsync(int userId); // pe cine urmareste X
        Task<int> GetFollowersCountAsync(int userId);
        Task<int> GetFollowingCountAsync(int userId);

    }

}
