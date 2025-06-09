using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.Common;
using KDomBackend.Models.DTOs.Post;
using KDomBackend.Models.MongoEntities;
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
        private readonly IUserProfileRepository _userProfileRepository;


        public PostReadService(IPostRepository repository,
            IUserService userService,
            IFollowRepository followRepository,
            IAuditLogRepository auditLogRepository,
            INotificationService notificationService,
            KDomTagHelper tagHelper,
            IKDomFollowRepository kdomFollowRepository,
            IKDomRepository kdomRepository,
            IUserProfileRepository userProfileRepository
            )
        {
            _repository = repository;
            _userService = userService;
            _followRepository = followRepository;
            _kdomFollowRepository = kdomFollowRepository;
            _kdomRepository = kdomRepository;
            _userProfileRepository = userProfileRepository;
        }

        public async Task<List<PostReadDto>> GetAllPostsAsync()
        {
            var posts = await _repository.GetAllAsync();
            var result = new List<PostReadDto>();

            foreach (var post in posts)
            {
                var username = await _userService.GetUsernameByUserIdAsync(post.UserId);
                var userProfile = await _userProfileRepository.GetProfileByUserIdAsync(post.UserId);
                var avatarUrl = userProfile?.AvatarUrl ?? "";

                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    UserId = post.UserId,
                    Username = username,
                    UserAvatarUrl = avatarUrl,
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
            var userProfile = await _userProfileRepository.GetProfileByUserIdAsync(post.UserId);
            var avatarUrl = userProfile?.AvatarUrl ?? "";

            return new PostReadDto
            {
                Id = post.Id,
                ContentHtml = post.ContentHtml,
                Tags = post.Tags,
                UserId = post.UserId,
                Username = username,
                UserAvatarUrl = avatarUrl,
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
            var userProfile = await _userProfileRepository.GetProfileByUserIdAsync(userId);
            var avatarUrl = userProfile?.AvatarUrl ?? "";

            return posts.Select(p => new PostReadDto
            {
                Id = p.Id,
                ContentHtml = p.ContentHtml,
                Tags = p.Tags,
                UserId = p.UserId,
                Username = username,
                UserAvatarUrl = avatarUrl,
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
                var userProfile = await _userProfileRepository.GetProfileByUserIdAsync(post.UserId);
                var avatarUrl = userProfile?.AvatarUrl ?? "";

                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    UserId = post.UserId,
                    Username = username,
                    UserAvatarUrl = avatarUrl,
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
                var userProfile = await _userProfileRepository.GetProfileByUserIdAsync(post.UserId);
                var avatarUrl = userProfile?.AvatarUrl ?? "";
                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    UserId = post.UserId,
                    Username = username,
                    UserAvatarUrl = avatarUrl,
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
                var userProfile = await _userProfileRepository.GetProfileByUserIdAsync(post.UserId);
                var avatarUrl = userProfile?.AvatarUrl ?? "";

                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    UserId = post.UserId,
                    Username = username ?? "unknown",
                    UserAvatarUrl = avatarUrl,
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    CreatedAt = post.CreatedAt
                });
            }

            return result;
        }

        public async Task<PagedResult<PostReadDto>> GetPostsByTagAsync(string tag, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            var totalCount = await _repository.GetCountByTagAsync(tag);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var posts = await _repository.GetByTagAsync(tag, skip, pageSize);
            var result = new List<PostReadDto>();

            foreach (var post in posts)
            {
                var username = await _userService.GetUsernameByUserIdAsync(post.UserId);
                var userProfile = await _userProfileRepository.GetProfileByUserIdAsync(post.UserId);
                var avatarUrl = userProfile?.AvatarUrl ?? "";


                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    UserId = post.UserId,
                    Username = username ?? "unknown",
                    UserAvatarUrl = avatarUrl,
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    CreatedAt = post.CreatedAt,
                    IsEdited = post.IsEdited,
                    EditedAt = post.EditedAt,
                    LikeCount = post.Likes.Count
                });
            }

            return new PagedResult<PostReadDto>
            {
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = totalPages,
                Items = result
            };
        }

        public async Task<int> GetPostsCountByTagAsync(string tag)
        {
            return await _repository.GetCountByTagAsync(tag);
        }
    }
}
