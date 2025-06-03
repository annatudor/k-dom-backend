using KDomBackend.Models.DTOs.Common;
using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Models.DTOs.Post;
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

        public async Task<KDomDiscussionReadDto> SearchKDomDiscussionAsync(string slug, KDomDiscussionSearchDto searchDto)
        {
            // 1. Verifică că K-Dom-ul există
            var kdom = await _kdomReadService.GetKDomBySlugAsync(slug);

            // 2. Efectuează căutarea în postări
            var pagedPosts = await SearchPostsWithFiltersAsync(slug, searchDto);

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

            // 4. Pentru search, nu calculăm statistici complete (pentru performanță)
            var searchStats = new KDomDiscussionStatsDto
            {
                TotalPosts = pagedPosts.TotalCount,
                TotalComments = 0, // Nu calculăm pentru search
                UniquePosterCount = 0, // Nu calculăm pentru search
                FirstPostDate = null,
                LastPostDate = null
            };

            return new KDomDiscussionReadDto
            {
                KDom = kdomBasic,
                Posts = pagedPosts,
                Stats = searchStats
            };
        }

        /// <summary>
        /// Metoda helper pentru căutarea cu filtre
        /// </summary>
        private async Task<PagedResult<PostReadDto>> SearchPostsWithFiltersAsync(string slug, KDomDiscussionSearchDto searchDto)
        {
            var skip = (searchDto.Page - 1) * searchDto.PageSize;

            // Folosim repository pentru căutarea avansată
            var posts = await _postRepository.SearchPostsByTagAsync(
                tag: slug.ToLower(),
                contentQuery: searchDto.ContentQuery,
                username: null, // O să filtrăm după username aici
                sortBy: searchDto.SortBy,
                onlyLiked: searchDto.OnlyLiked,
                lastDays: searchDto.LastDays,
                skip: skip,
                limit: searchDto.PageSize
            );

            // Obținem total count pentru paginare
            var totalCount = await _postRepository.CountSearchPostsByTagAsync(
                tag: slug.ToLower(),
                contentQuery: searchDto.ContentQuery,
                username: null,
                onlyLiked: searchDto.OnlyLiked,
                lastDays: searchDto.LastDays
            );

            var result = new List<PostReadDto>();

            foreach (var post in posts)
            {
                var username = await _userService.GetUsernameByUserIdAsync(post.UserId);

                // Filtrăm după username dacă este specificat
                if (!string.IsNullOrWhiteSpace(searchDto.Username) &&
                    !username.ToLower().Contains(searchDto.Username.ToLower()))
                {
                    continue;
                }

                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    UserId = post.UserId,
                    Username = username ?? "unknown",
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    CreatedAt = post.CreatedAt,
                    IsEdited = post.IsEdited,
                    EditedAt = post.EditedAt,
                    LikeCount = post.Likes.Count
                });
            }

            var totalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize);

            return new PagedResult<PostReadDto>
            {
                TotalCount = totalCount,
                PageSize = searchDto.PageSize,
                CurrentPage = searchDto.Page,
                TotalPages = totalPages,
                Items = result
            };
        }

    }
}