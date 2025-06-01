using KDomBackend.Enums;
using KDomBackend.Models.MongoEntities;

namespace KDomBackend.Repositories.Interfaces
{
    public interface IViewTrackingRepository
    {
        Task CreateAsync(ViewTracking viewTracking);
        Task<int> GetViewCountAsync(ContentType contentType, string contentId);
        Task<int> GetUserContentViewsAsync(int userId, ContentType contentType);
        Task<Dictionary<string, int>> GetUserContentViewsBreakdownAsync(int userId, ContentType contentType);
        Task<bool> HasRecentViewAsync(ContentType contentType, string contentId, int? userId, string? ipAddress, int minutesWindow = 30);
        Task<Dictionary<string, int>> GetTopViewedContentAsync(ContentType contentType, int limit = 10);
        Task<List<ViewTracking>> GetRecentViewsAsync(int? userId, int limit = 20);
        Task<int> GetTotalViewsByUserAsync(int userId); // Views pe tot conținutul unui user
    }
}