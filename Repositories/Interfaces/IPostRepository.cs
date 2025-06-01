using KDomBackend.Models.MongoEntities;

namespace KDomBackend.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task CreateAsync(Post post);
        Task<List<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(string id);
        Task ToggleLikeAsync(string postId, int userId, bool like);
        Task UpdateAsync(string postId, string newHtml, List<string> newTags);
        Task DeleteAsync(string postId);
        Task<List<Post>> GetByUserIdAsync(int userId);
        Task<List<Post>> GetFeedPostsAsync(List<int> followedUserIds, List<string> followedTags, int limit = 30);
        Task<List<Post>> GetPublicPostsAsync(int limit = 30);
        Task<List<Post>> GetByTagAsync(string tag);
        Task<Dictionary<string, int>> GetRecentTagCountsAsync(int days = 7);
        Task<List<Post>> GetRecentPostsByUserAsync(int userId, int limit = 30);

        Task<int> GetPostCountByUserAsync(int userId);
        Task<int> GetTotalLikesReceivedByUserPostsAsync(int userId);
        Task<int> GetTotalLikesGivenByUserAsync(int userId);
        Task<List<Post>> GetPostsByUserAsync(int userId, int limit = 50);
        Task<Dictionary<string, int>> GetUserPostsByMonthAsync(int userId, int months = 12);
        Task<List<string>> GetUserPostIdsAsync(int userId); // Pentru a obține comment count


    }
}
