using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class KDomDiscussionService : IKDomDiscussionService
    {
        private readonly IKDomReadService _kdomReadService;
        private readonly IPostReadService _postReadService;
        private readonly IKDomFollowService _kdomFollowService;
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUserService _userService;

        public KDomDiscussionService(
            IKDomReadService kdomReadService,
            IPostReadService postReadService,
            IKDomFollowService kdomFollowService,
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            IUserService userService)
        {
            _kdomReadService = kdomReadService;
            _postReadService = postReadService;
            _kdomFollowService = kdomFollowService;
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _userService = userService;
        }

        public async Task<KDomDiscussionReadDto> GetKDomDiscussionAsync(string slug, KDomDiscussionFilterDto filter)
        {
            // 1. Verifică că K-Dom-ul există
            var kdom = await _kdomReadService.GetKDomBySlugAsync(slug);

            // 2. Obține postările cu paginare
            var pagedPosts = await _postReadService.GetPostsByTagAsync(slug.ToLower(), filter.Page, filter.PageSize);

            // 3. Obține informații de bază despre K-Dom
            var followersCount = await _kdomFollowService.GetFollowersCountAsync(kdom.Id);

            var kdomBasic = new KDomBasicInfoDto
            {
                Id = kdom.Id,
                Title = kdom.Title,
                Slug = kdom.Slug,
                Description = kdom.Description,
                AuthorUsername = kdom.AuthorUsername,
                FollowersCount = followersCount
            };

            // 4. Obține statistici discussion
            var stats = await GetDiscussionStatsAsync(slug);

            return new KDomDiscussionReadDto
            {
                KDom = kdomBasic,
                Posts = pagedPosts,
                Stats = stats
            };
        }

        public async Task<KDomDiscussionStatsDto> GetDiscussionStatsAsync(string slug)
        {
            var totalPosts = await _postReadService.GetPostsCountByTagAsync(slug.ToLower());

            // Obține toate postările pentru a calcula statistici
            var allPosts = await _postReadService.GetPostsByTagAsync(slug.ToLower());

            var totalComments = 0;
            var uniquePosters = new HashSet<int>();
            DateTime? firstPostDate = null;
            DateTime? lastPostDate = null;

            foreach (var post in allPosts)
            {
                uniquePosters.Add(post.UserId);

                if (firstPostDate == null || post.CreatedAt < firstPostDate)
                    firstPostDate = post.CreatedAt;

                if (lastPostDate == null || post.CreatedAt > lastPostDate)
                    lastPostDate = post.CreatedAt;

                // Calculează comentariile pentru acest post
                var comments = await _commentRepository.GetByTargetAsync(
                    Enums.CommentTargetType.Post,
                    post.Id
                );
                totalComments += comments.Count;
            }

            return new KDomDiscussionStatsDto
            {
                TotalPosts = totalPosts,
                TotalComments = totalComments,
                UniquePosterCount = uniquePosters.Count,
                FirstPostDate = firstPostDate,
                LastPostDate = lastPostDate
            };
        }

        public async Task<bool> HasActiveDiscussionAsync(string slug)
        {
            var totalPosts = await _postReadService.GetPostsCountByTagAsync(slug.ToLower());
            return totalPosts > 0;
        }
    }
}