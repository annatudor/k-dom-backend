using KDomBackend.Enums;
using KDomBackend.Models.MongoEntities;

namespace KDomBackend.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task CreateAsync(Comment comment);
        Task<List<Comment>> GetByTargetAsync(CommentTargetType type, string targetId);
        Task<List<Comment>> GetRepliesAsync(string parentCommentId);
        Task<Comment?> GetByIdAsync(string id);
        Task UpdateTextAsync(string id, string newText);
        Task DeleteAsync(string id);
        Task ToggleLikeAsync(string commentId, int userId, bool like);
        Task<Dictionary<string, int>> CountRecentCommentsByKDomAsync(int days = 7);
        Task<List<string>> GetCommentedKDomIdsByUserAsync(int userId, int days = 30);


        Task<int> GetCommentCountByUserAsync(int userId);
        Task<int> GetCommentsReceivedByUserAsync(int userId); // Comentarii primite pe posturile user-ului
        Task<List<Comment>> GetCommentsByUserAsync(int userId, int limit = 50);
        Task<int> GetTotalLikesReceivedByUserCommentsAsync(int userId);
        Task<int> GetTotalLikesGivenByUserAsync(int userId);

        Task<List<Comment>> GetCommentsOnUserKDomsAsync(List<string> kdomIds, int limit = 100);
        Task<Dictionary<string, int>> GetUserCommentsByMonthAsync(int userId, int months = 12);
        Task<Dictionary<int, int>> GetTopCommentersOnUserContentAsync(int userId, List<string> userContentIds, int limit = 10);
    }
}
