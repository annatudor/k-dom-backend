// Services/Interfaces/IViewTrackingService.cs - Interfața service actualizată
using KDomBackend.Enums;
using KDomBackend.Models.DTOs.ViewTracking;

namespace KDomBackend.Services.Interfaces
{
    public interface IViewTrackingService
    {
       

        Task TrackViewAsync(ViewTrackingCreateDto dto);

        Task<int> GetContentViewCountAsync(ContentType contentType, string contentId);

        Task<int> GetRecentViewsAsync(ContentType contentType, string contentId, int hours = 24);
 
        Task<int> GetUniqueViewersAsync(ContentType contentType, string contentId);

        Task<ViewStatsDto> GetDetailedStatsAsync(ContentType contentType, string contentId);

        Task<Dictionary<string, int>> GetTopViewedContentAsync(ContentType contentType, int limit = 10);

        Task<List<TrendingContentDto>> GetTrendingContentAsync(ContentType? contentType = null, int hours = 24, int limit = 10);

        Task<int> GetUserTotalViewsAsync(int userId);
     
        Task<Dictionary<string, int>> GetUserViewsBreakdownAsync(int userId);

        Task<int> GetTotalViewsAsync(int days = 30);
        Task<Dictionary<string, int>> GetViewsByContentTypeAsync(int days = 30);

        Task<Dictionary<string, int>> GetDailyViewsAsync(int days = 30);

        Task<AnalyticsDto> GetAnalyticsAsync(int days = 30);


    }
}