using KDomBackend.Models.DTOs.User;

namespace KDomBackend.Services.Interfaces
{
    public interface IFollowService
    {
        Task FollowUserAsync(int followerId, int followingId);
        Task UnfollowUserAsync(int followerId, int followingId);
        Task<List<UserPublicDto>> GetFollowersAsync(int userId);
        Task<List<UserPublicDto>> GetFollowingAsync(int userId);
        Task<int> GetFollowersCountAsync(int userId);
        Task<int> GetFollowingCountAsync(int userId);

    }

}
