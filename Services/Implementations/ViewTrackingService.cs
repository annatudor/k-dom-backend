// Services/Implementations/ViewTrackingService.cs - Versiunea îmbunătățită
using KDomBackend.Enums;
using KDomBackend.Models.DTOs.ViewTracking;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class ViewTrackingService : IViewTrackingService
    {
        private readonly IViewTrackingRepository _viewTrackingRepository;
        private readonly IPostRepository _postRepository;
        private readonly IKDomRepository _kdomRepository;

        public ViewTrackingService(
            IViewTrackingRepository viewTrackingRepository,
            IPostRepository postRepository,
            IKDomRepository kdomRepository)
        {
            _viewTrackingRepository = viewTrackingRepository;
            _postRepository = postRepository;
            _kdomRepository = kdomRepository;
        }

        public async Task TrackViewAsync(ViewTrackingCreateDto dto)
        {
            // Verifică dacă nu a fost o vizualizare recentă pentru a evita spam-ul
            var hasRecentView = await _viewTrackingRepository.HasRecentViewAsync(
                dto.ContentType,
                dto.ContentId,
                dto.ViewerId,
                dto.IpAddress,
                30); // 30 minute window

            if (hasRecentView)
                return; // Nu înregistra view duplicate

            var viewTracking = new ViewTracking
            {
                ContentType = dto.ContentType,
                ContentId = dto.ContentId,
                ViewerId = dto.ViewerId,
                IpAddress = dto.IpAddress,
                UserAgent = dto.UserAgent,
                ViewedAt = DateTime.UtcNow,
                IsUnique = true
            };

            await _viewTrackingRepository.CreateAsync(viewTracking);
        }

        public async Task<int> GetContentViewCountAsync(ContentType contentType, string contentId)
        {
            return await _viewTrackingRepository.GetViewCountAsync(contentType, contentId);
        }

        public async Task<int> GetUserTotalViewsAsync(int userId)
        {
            // Calculează total views pe toate postările și K-Dom-urile user-ului
            var userPostViews = await GetUserPostViewsAsync(userId);
            var userKDomViews = await GetUserKDomViewsAsync(userId);

            return userPostViews + userKDomViews;
        }

        public async Task<Dictionary<string, int>> GetUserViewsBreakdownAsync(int userId)
        {
            var postViews = await GetUserPostViewsAsync(userId);
            var kdomViews = await GetUserKDomViewsAsync(userId);

            return new Dictionary<string, int>
            {
                { "Posts", postViews },
                { "KDoms", kdomViews },
                { "Total", postViews + kdomViews }
            };
        }

        public async Task<Dictionary<string, int>> GetTopViewedContentAsync(ContentType contentType, int limit = 10)
        {
            return await _viewTrackingRepository.GetTopViewedContentAsync(contentType, limit);
        }

        // ✅ METODE NOI pentru funcționalități îmbunătățite

        /// <summary>
        /// Obține numărul de view-uri recente (ultimele X ore)
        /// </summary>
        public async Task<int> GetRecentViewsAsync(ContentType contentType, string contentId, int hours = 24)
        {
            return await _viewTrackingRepository.GetRecentViewsCountAsync(contentType, contentId, hours);
        }

        /// <summary>
        /// Obține numărul de unique viewers pentru un conținut
        /// </summary>
        public async Task<int> GetUniqueViewersAsync(ContentType contentType, string contentId)
        {
            return await _viewTrackingRepository.GetUniqueViewersCountAsync(contentType, contentId);
        }

        /// <summary>
        /// Obține statistici detaliate pentru un conținut
        /// </summary>
        public async Task<ViewStatsDto> GetDetailedStatsAsync(ContentType contentType, string contentId)
        {
            var viewCount = await GetContentViewCountAsync(contentType, contentId);
            var recentViews = await GetRecentViewsAsync(contentType, contentId, 24);
            var uniqueViewers = await GetUniqueViewersAsync(contentType, contentId);
            var lastViewed = await _viewTrackingRepository.GetLastViewedDateAsync(contentType, contentId);
            
            // Calculează growth rate (dummy calculation - ar trebui să compare cu perioada anterioară)
            var previousPeriodViews = await GetRecentViewsAsync(contentType, contentId, 48) - recentViews;
            var growthRate = previousPeriodViews > 0 ? ((double)(recentViews - previousPeriodViews) / previousPeriodViews) * 100 : 0;

            // Determină popularity level
            var popularityLevel = GetPopularityLevel(viewCount);

            return new ViewStatsDto
            {
                ContentType = contentType,
                ContentId = contentId,
                ViewCount = viewCount,
                RecentViews = recentViews,
                UniqueViewers = uniqueViewers,
                LastViewed = lastViewed,
                GrowthRate = growthRate,
                PopularityLevel = popularityLevel
            };
        }

        /// <summary>
        /// Obține conținut trending bazat pe view-urile recente
        /// </summary>
        public async Task<List<TrendingContentDto>> GetTrendingContentAsync(ContentType? contentType = null, int hours = 24, int limit = 10)
        {
            return await _viewTrackingRepository.GetTrendingContentAsync(contentType, hours, limit);
        }

        /// <summary>
        /// Obține total views pentru o perioadă specificată
        /// </summary>
        public async Task<int> GetTotalViewsAsync(int days = 30)
        {
            return await _viewTrackingRepository.GetTotalViewsForPeriodAsync(days);
        }

        /// <summary>
        /// Obține distribuția view-urilor pe tipuri de conținut
        /// </summary>
        public async Task<Dictionary<string, int>> GetViewsByContentTypeAsync(int days = 30)
        {
            return await _viewTrackingRepository.GetViewsByContentTypeAsync(days);
        }

        /// <summary>
        /// Obține view-urile zilnice pentru o perioadă
        /// </summary>
        public async Task<Dictionary<string, int>> GetDailyViewsAsync(int days = 30)
        {
            return await _viewTrackingRepository.GetDailyViewsAsync(days);
        }

        /// <summary>
        /// Obține analytics complete pentru dashboard
        /// </summary>
        public async Task<AnalyticsDto> GetAnalyticsAsync(int days = 30)
        {
            var totalViews = await GetTotalViewsAsync(days);
            var viewsByType = await GetViewsByContentTypeAsync(days);
            var topKDomsDict = await GetTopViewedContentAsync(ContentType.KDom, 10);
            var topPostsDict = await GetTopViewedContentAsync(ContentType.Post, 10);
            var dailyViews = await GetDailyViewsAsync(days);

            // Convert dictionaries to DTO lists
            var topKDoms = await ConvertToTopContentDto(topKDomsDict, ContentType.KDom);
            var topPosts = await ConvertToTopContentDto(topPostsDict, ContentType.Post);

            var trends = await CalculateViewTrends(days);

            return new AnalyticsDto
            {
                PeriodDays = days,
                TotalViews = totalViews,
                ViewsByType = viewsByType,
                TopKDoms = topKDoms,
                TopPosts = topPosts,
                DailyViews = dailyViews,
                Trends = trends
            };
        }

        // ✅ METODE HELPER PRIVATE

        private async Task<int> GetUserPostViewsAsync(int userId)
        {
            // Obține toate post-urile user-ului
            var userPostIds = await _postRepository.GetUserPostIdsAsync(userId);

            int totalViews = 0;
            foreach (var postId in userPostIds)
            {
                var views = await _viewTrackingRepository.GetViewCountAsync(ContentType.Post, postId);
                totalViews += views;
            }

            return totalViews;
        }

        private async Task<int> GetUserKDomViewsAsync(int userId)
        {
            // Obține toate K-Dom-urile user-ului
            var userKDomIds = await _kdomRepository.GetUserKDomIdsAsync(userId, false); // Doar create, nu colaborate

            int totalViews = 0;
            foreach (var kdomId in userKDomIds)
            {
                var views = await _viewTrackingRepository.GetViewCountAsync(ContentType.KDom, kdomId);
                totalViews += views;
            }

            return totalViews;
        }

        private string GetPopularityLevel(int viewCount)
        {
            return viewCount switch
            {
                >= 10000 => "Viral",
                >= 1000 => "Popular",
                >= 100 => "Growing",
                >= 10 => "Active",
                _ => "New"
            };
        }

        private async Task<List<TopContentDto>> ConvertToTopContentDto(Dictionary<string, int> viewData, ContentType contentType)
        {
            var result = new List<TopContentDto>();

            foreach (var kvp in viewData)
            {
                var contentId = kvp.Key;
                var viewCount = kvp.Value;

                string title = "Unknown";
                string? slug = null;
                DateTime createdAt = DateTime.UtcNow;

                try
                {
                    if (contentType == ContentType.KDom)
                    {
                        var kdom = await _kdomRepository.GetByIdAsync(contentId);
                        if (kdom != null)
                        {
                            title = kdom.Title;
                            slug = kdom.Slug;
                            createdAt = kdom.CreatedAt;
                        }
                    }
                    else if (contentType == ContentType.Post)
                    {
                        var post = await _postRepository.GetByIdAsync(contentId);
                        if (post != null)
                        {
                            // Pentru posts, folosim primele 50 de caractere ca titlu
                            title = post.ContentHtml.Length > 50 ? 
                                post.ContentHtml.Substring(0, 50) + "..." : 
                                post.ContentHtml;
                            createdAt = post.CreatedAt;
                        }
                    }
                }
                catch
                {
                    // Ignoră erorile și folosește valorile default
                }

                result.Add(new TopContentDto
                {
                    ContentId = contentId,
                    ContentType = contentType,
                    Title = title,
                    ViewCount = viewCount,
                    CreatedAt = createdAt,
                    Slug = slug
                });
            }

            return result.OrderByDescending(x => x.ViewCount).ToList();
        }

        private async Task<ViewTrendsDto> CalculateViewTrends(int days)
        {
            // Implementare simplă - ar putea fi îmbunătățită cu calcule mai complexe
            var currentPeriodViews = await GetTotalViewsAsync(days);
            var previousPeriodViews = await _viewTrackingRepository.GetTotalViewsForPeriodAsync(days, days);
            
            var growthRate = previousPeriodViews > 0 ? 
                ((double)(currentPeriodViews - previousPeriodViews) / previousPeriodViews) * 100 : 0;

            return new ViewTrendsDto
            {
                GrowthRate = growthRate,
                MostActiveHour = 14, // Placeholder - ar trebui calculat din date reale
                MostActiveDay = "Monday", // Placeholder
                PeakViews = currentPeriodViews,
                PeakViewsDate = DateTime.UtcNow.Date,
                ViewDistribution = new Dictionary<string, double>() // Placeholder
            };
        }
    }
}