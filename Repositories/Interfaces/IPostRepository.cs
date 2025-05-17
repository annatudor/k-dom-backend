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

    }
}
