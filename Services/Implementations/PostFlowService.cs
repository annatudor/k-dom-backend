using KDomBackend.Enums;
using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.Notification;
using KDomBackend.Models.DTOs.Post;
using KDomBackend.Models.Entities;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class PostFlowService : IPostFlowService
    {
        private readonly IPostRepository _repository;
        private readonly IUserService _userService;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly INotificationService _notificationService;
        private readonly KDomTagHelper _tagHelper;
        private readonly IKDomRepository _kdomRepository;



        public PostFlowService(IPostRepository repository,
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
            _auditLogRepository = auditLogRepository;
            _notificationService = notificationService;
            _tagHelper = tagHelper;
            _kdomRepository = kdomRepository;
        }

        public async Task CreatePostAsync(PostCreateDto dto, int userId)
        {
            var cleanHtml = HtmlSanitizerHelper.Sanitize(dto.ContentHtml);

            // FIXED: Process tags correctly
            List<string> tags = new List<string>();

            // If KDomId is provided, get tags from that K-Dom
            if (!string.IsNullOrEmpty(dto.KDomId))
            {
                var kdomTags = await _tagHelper.GetTagsFromKDomIdAsync(dto.KDomId);
                tags.AddRange(kdomTags);
            }

            // If additional tags are provided from frontend, validate and add them
            if (dto.Tags != null && dto.Tags.Any())
            {
                foreach (var tagSlug in dto.Tags)
                {
                    if (!string.IsNullOrWhiteSpace(tagSlug))
                    {
                        // Verify that the tag corresponds to an actual K-Dom
                        var kdom = await _kdomRepository.GetBySlugAsync(tagSlug);
                        if (kdom != null && !tags.Contains(tagSlug))
                        {
                            tags.Add(tagSlug);
                        }
                    }
                }
            }

            var post = new Post
            {
                UserId = userId,
                ContentHtml = cleanHtml,
                Tags = tags, // Now properly populated
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(post);

            // Handle mentions if content contains @
            if (dto.ContentHtml.Contains("@"))
            {
                await MentionHelper.HandleMentionsAsync(
                    dto.ContentHtml,
                    userId,
                    post.Id,
                    ContentType.Post,
                    NotificationType.MentionInPost,
                    _userService,
                    _notificationService);
            }
        }
        public async Task<PostLikeResponseDto> ToggleLikeAsync(string postId, int userId)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null)
                throw new Exception("Post not found.");

            var alreadyLiked = post.Likes.Contains(userId);
            var like = !alreadyLiked;

            await _repository.ToggleLikeAsync(postId, userId, like);

            if (like && post.UserId != userId)
            {
                var likerUsername = await _userService.GetUsernameByUserIdAsync(userId);

                await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = post.UserId,
                    Type = NotificationType.PostLiked,
                    Message = $"{likerUsername} liked your post.",
                    TriggeredByUserId = userId,
                    TargetType = ContentType.Post,
                    TargetId = postId
                });
            }

            var updatedPost = await _repository.GetByIdAsync(postId);
            var count = updatedPost?.Likes.Count ?? 0;

            return new PostLikeResponseDto
            {
                Liked = like,
                LikeCount = count
            };
        }
        public async Task EditPostAsync(string postId, PostEditDto dto, int userId)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null)
                throw new Exception("Post not found.");

            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You cannot edit this post.");

            var cleanHtml = HtmlSanitizerHelper.Sanitize(dto.ContentHtml);

            if (cleanHtml == post.ContentHtml && dto.Tags.SequenceEqual(post.Tags))
                return; // nimic de modificat

            await _repository.UpdateAsync(postId, cleanHtml, dto.Tags);
        }

        public async Task DeletePostAsync(string postId, int userId, bool isModerator)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null) throw new Exception("Post not found.");

            // autorul are voie oricum
            var isOwner = post.UserId == userId;

            if (!isOwner && !isModerator)
                throw new UnauthorizedAccessException("You cannot have permission to delete this post.");

            // stergere postare
            await _repository.DeleteAsync(postId);

            // audit obligatoriu
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                Action = AuditAction.DeletePost,
                TargetType = AuditTargetType.Post,
                TargetId = postId,
                CreatedAt = DateTime.UtcNow,
                Details = isModerator ? "Deleted by moderator" : "Deleted by user"
            });

            // notificare doar daca e moderator
            if (isModerator)
            {
                await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = post.UserId,
                    Type = NotificationType.SystemMessage,
                    Message = "Your post has been deleted by a moderator.",
                    TriggeredByUserId = userId,
                    TargetType = ContentType.Post,
                    TargetId = postId
                });
            }
        }
    }
}
