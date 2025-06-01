using KDomBackend.Enums;
using KDomBackend.Models.DTOs.ViewTracking;

namespace KDomBackend.Services.Interfaces
{
    public interface IViewTrackingService
    {
        Task TrackViewAsync(ViewTrackingCreateDto dto);
        Task<int> GetContentViewCountAsync(ContentType contentType, string contentId);
        Task<int> GetUserTotalViewsAsync(int userId); 
        Task<Dictionary<string, int>> GetUserViewsBreakdownAsync(int userId); 
        Task<Dictionary<string, int>> GetTopViewedContentAsync(ContentType contentType, int limit = 10);
    }
}
