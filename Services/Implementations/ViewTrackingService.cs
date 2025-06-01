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
    }
}