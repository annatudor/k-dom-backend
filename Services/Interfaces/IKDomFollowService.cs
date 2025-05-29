using KDomBackend.Models.DTOs.KDom;

namespace KDomBackend.Services.Interfaces
{
    public interface IKDomFollowService
    {
        Task FollowAsync(string kdomId, int userId);
        Task UnfollowAsync(string kdomId, int userId);
        Task<List<KDomTagSearchResultDto>> GetFollowedKDomsAsync(int userId);
        Task<bool> IsFollowingAsync(string kdomId, int userId);
        Task<int> GetFollowersCountAsync(string kdomId);


    }

}
