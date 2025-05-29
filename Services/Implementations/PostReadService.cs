using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.Post;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class PostReadService : IPostReadService
    {
        private readonly IPostRepository _repository;
        private readonly IUserService _userService;
        private readonly IFollowRepository _followRepository;
        private readonly IKDomFollowRepository _kdomFollowRepository;
        private readonly IKDomRepository _kdomRepository;


        public PostReadService(IPostRepository repository,
            IUserService userService,
            IFollowRepository followRepository,
            IAuditLogRepository auditLogRepository,
            INotificationService notificationService,
            KDomTagHelper tagHelper,
            IKDomFollowRepository kdomFollowRepository,
            IKDomRepository kdomRepository
            )
        {
            _repository = repository;
            _userService = userService;
            _followRepository = followRepository;
            _kdomFollowRepository = kdomFollowRepository;
            _kdomRepository = kdomRepository;
        }

        public async Task<List<PostReadDto>> GetAllPostsAsync()
        {
            var posts = await _repository.GetAllAsync();
            var result = new List<PostReadDto>();

            foreach (var post in posts)
            {
                var username = await _userService.GetUsernameByUserIdAsync(post.UserId);

                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    UserId = post.UserId,
                    Username = username,
                    CreatedAt = post.CreatedAt,
                    IsEdited = post.IsEdited,
                    EditedAt = post.EditedAt,
                    LikeCount = post.Likes.Count
                });

            }

            return result;
        }
        public async Task<PostReadDto> GetByIdAsync(string postId)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null)
                throw new Exception("Post not found.");

            var username = await _userService.GetUsernameByUserIdAsync(post.UserId);

            return new PostReadDto
            {
                Id = post.Id,
                ContentHtml = post.ContentHtml,
                Tags = post.Tags,
                UserId = post.UserId,
                Username = username,
                CreatedAt = post.CreatedAt,
                IsEdited = post.IsEdited,
                EditedAt = post.EditedAt,
                LikeCount = post.Likes.Count
            };
        }

        public async Task<List<PostReadDto>> GetPostsByUserIdAsync(int userId)
        {
            var posts = await _repository.GetByUserIdAsync(userId);
            var username = await _userService.GetUsernameByUserIdAsync(userId);

            return posts.Select(p => new PostReadDto
            {
                Id = p.Id,
                ContentHtml = p.ContentHtml,
                Tags = p.Tags,
                UserId = p.UserId,
                Username = username,
                CreatedAt = p.CreatedAt,
                IsEdited = p.IsEdited,
                EditedAt = p.EditedAt,
                LikeCount = p.Likes.Count
            }).ToList();
        }

        public async Task<List<PostReadDto>> GetFeedAsync(int userId)
        {
            var followedUserIds = await _followRepository.GetFollowingAsync(userId);
            var followedKDomIds = await _kdomFollowRepository.GetFollowedKDomIdsAsync(userId);
            var followedKdoms = await _kdomRepository.GetByIdsAsync(followedKDomIds);
            var followedTags = followedKdoms.Select(k => k.Slug).ToList();

            if (!followedUserIds.Any() && !followedKDomIds.Any())
            {
                // un array gol
                return new List<PostReadDto>();
            }

            var posts = await _repository.GetFeedPostsAsync(followedUserIds, followedTags);

            var result = new List<PostReadDto>();
            foreach (var post in posts)
            {
                var username = await _userService.GetUsernameByUserIdAsync(post.UserId);

                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    UserId = post.UserId,
                    Username = username,
                    CreatedAt = post.CreatedAt,
                    IsEdited = post.IsEdited,
                    EditedAt = post.EditedAt,
                    LikeCount = post.Likes.Count
                });
            }

            return result;
        }


        public async Task<List<PostReadDto>> GetGuestFeedAsync(int limit = 30)
        {
            var posts = await _repository.GetPublicPostsAsync(limit);
            var result = new List<PostReadDto>();

            foreach (var post in posts)
            {
                var username = await _userService.GetUsernameByUserIdAsync(post.UserId);

                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    UserId = post.UserId,
                    Username = username,
                    CreatedAt = post.CreatedAt,
                    IsEdited = post.IsEdited,
                    EditedAt = post.EditedAt,
                    LikeCount = post.Likes.Count
                });
            }

            return result;
        }

        public async Task<List<PostReadDto>> GetPostsByTagAsync(string tag)
        {
            var posts = await _repository.GetByTagAsync(tag);
            var result = new List<PostReadDto>();

            foreach (var post in posts)
            {
                var username = await _userService.GetUsernameByUserIdAsync(post.UserId);

                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    UserId = post.UserId,
                    Username = username ?? "unknown",
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    CreatedAt = post.CreatedAt
                });
            }

            return result;
        }
    }
}
