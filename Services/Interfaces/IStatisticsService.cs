// Services/Interfaces/IStatisticsService.cs
using KDomBackend.Models.DTOs.Statistics;

namespace KDomBackend.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<PlatformStatsDto> GetPlatformStatsAsync();
        Task<List<CategoryStatsDto>> GetCategoryStatsAsync();
        Task<List<FeaturedKDomForHomepageDto>> GetFeaturedKDomsAsync(int limit = 6);
        Task<HomepageDataDto> GetHomepageDataAsync();
    }
}