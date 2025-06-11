// Services/Implementations/StatisticsService.cs
using KDomBackend.Models.DTOs.Statistics;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IKDomRepository _kdomRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IKDomFollowRepository _kdomFollowRepository;

        public StatisticsService(
            IKDomRepository kdomRepository,
            IUserRepository userRepository,
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            IKDomFollowRepository kdomFollowRepository)
        {
            _kdomRepository = kdomRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _kdomFollowRepository = kdomFollowRepository;
        }

        public async Task<PlatformStatsDto> GetPlatformStatsAsync()
        {
            // Obține toate K-Dom-urile aprobate
            var approvedKDoms = await _kdomRepository.GetApprovedKdomsAsync();

            // Calculează colaboratorii activi unici
            var uniqueCollaborators = approvedKDoms
                .SelectMany(k => k.Collaborators)
                .Distinct()
                .Count();

            // Adaugă și autorii K-Dom-urilor
            var authors = approvedKDoms.Select(k => k.UserId).Distinct().Count();
            var totalActiveCollaborators = uniqueCollaborators + authors;

            // Calculează numărul de categorii cu conținut
            var categoriesWithContent = approvedKDoms
                .GroupBy(k => k.Hub)
                .Count();

            return new PlatformStatsDto
            {
                TotalKDoms = approvedKDoms.Count,
                TotalCategories = Math.Max(categoriesWithContent, 6), // Minimum 6 categorii
                ActiveCollaborators = totalActiveCollaborators,
                TotalUsers = await _userRepository.GetTotalUsersCountAsync(),
                TotalPosts = await _postRepository.GetTotalPostsCountAsync(),
                TotalComments = await _commentRepository.GetTotalCommentsCountAsync()
            };
        }

        public async Task<List<CategoryStatsDto>> GetCategoryStatsAsync()
        {
            // Obține toate K-Dom-urile aprobate
            var approvedKDoms = await _kdomRepository.GetApprovedKdomsAsync();

            // Grupează pe hub
            var categoryGroups = approvedKDoms
                .GroupBy(k => k.Hub)
                .ToList();

            var categoryStats = new List<CategoryStatsDto>();

            foreach (var group in categoryGroups)
            {
                var kdomsInCategory = group.ToList();

                // Obține top 3 K-Dom-uri din această categorie (după numărul de followers)
                var featuredKDoms = new List<FeaturedKDomDto>();

                foreach (var kdom in kdomsInCategory.Take(3))
                {
                    var followersCount = await _kdomFollowRepository.GetFollowersCountAsync(kdom.Id);
                    featuredKDoms.Add(new FeaturedKDomDto
                    {
                        Id = kdom.Id,
                        Title = kdom.Title,
                        Slug = kdom.Slug,
                        FollowersCount = followersCount
                    });
                }

                // Sortează featured K-Dom-urile după followers
                featuredKDoms = featuredKDoms
                    .OrderByDescending(f => f.FollowersCount)
                    .ToList();

                categoryStats.Add(new CategoryStatsDto
                {
                    Hub = group.Key.ToString(),
                    Count = kdomsInCategory.Count,
                    Featured = featuredKDoms
                });
            }

            // Sortează categoriile după numărul de K-Dom-uri
            return categoryStats
                .OrderByDescending(c => c.Count)
                .ToList();
        }

        public async Task<List<FeaturedKDomForHomepageDto>> GetFeaturedKDomsAsync(int limit = 6)
        {
            // Obține K-Dom-urile trending
            var trending = await _kdomRepository.GetApprovedKdomsAsync();

            var featuredKDoms = new List<FeaturedKDomForHomepageDto>();

            foreach (var kdom in trending.Take(limit))
            {
                // Calculează un score bazat pe followers, posts, etc.
                var followersCount = await _kdomFollowRepository.GetFollowersCountAsync(kdom.Id);
                var postsCount = await _postRepository.GetPostCountByTagAsync(kdom.Slug);
                var commentsCount = await _commentRepository.GetCommentsCountByKDomAsync(kdom.Id);

                // Score simplu: followers * 3 + posts * 2 + comments
                var score = (followersCount * 3) + (postsCount * 2) + commentsCount;

                featuredKDoms.Add(new FeaturedKDomForHomepageDto
                {
                    Id = kdom.Id,
                    Title = kdom.Title,
                    Slug = kdom.Slug,
                    Score = score,
                    Hub = kdom.Hub.ToString(),
                    FollowersCount = followersCount,
                    PostsCount = postsCount,
                    CommentsCount = commentsCount
                });
            }

            // Sortează după score
            return featuredKDoms
                .OrderByDescending(f => f.Score)
                .Take(limit)
                .ToList();
        }

        public async Task<HomepageDataDto> GetHomepageDataAsync()
        {
            var platformStats = await GetPlatformStatsAsync();
            var categoryStats = await GetCategoryStatsAsync();
            var featuredKDoms = await GetFeaturedKDomsAsync(6);

            return new HomepageDataDto
            {
                PlatformStats = platformStats,
                CategoryStats = categoryStats,
                FeaturedKDoms = featuredKDoms
            };
        }
    }
}