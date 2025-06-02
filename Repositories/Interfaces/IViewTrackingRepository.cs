// Repositories/Interfaces/IViewTrackingRepository.cs - Interfața esențială
using KDomBackend.Enums;
using KDomBackend.Models.DTOs.ViewTracking;
using KDomBackend.Models.MongoEntities;

namespace KDomBackend.Repositories.Interfaces
{
    public interface IViewTrackingRepository
    {
       
        Task CreateAsync(ViewTracking viewTracking);

        Task<int> GetViewCountAsync(ContentType contentType, string contentId);

      
        Task<bool> HasRecentViewAsync(ContentType contentType, string contentId, int? userId, string? ipAddress, int minutesWindow = 30);

        
        Task<int> GetRecentViewsCountAsync(ContentType contentType, string contentId, int hours = 24);

       
        Task<int> GetUniqueViewersCountAsync(ContentType contentType, string contentId);


        Task<DateTime?> GetLastViewedDateAsync(ContentType contentType, string contentId);

      
        Task<Dictionary<string, int>> GetTopViewedContentAsync(ContentType contentType, int limit = 10);

      
        Task<List<TrendingContentDto>> GetTrendingContentAsync(ContentType? contentType = null, int hours = 24, int limit = 10);

      
        Task<int> GetTotalViewsForPeriodAsync(int days, int offsetDays = 0);

       
        Task<Dictionary<string, int>> GetViewsByContentTypeAsync(int days = 30);

        Task<Dictionary<string, int>> GetDailyViewsAsync(int days = 30);

      
    }
}