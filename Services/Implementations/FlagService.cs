// Services/Implementations/FlagService.cs - Fixed version pentru DbEnum
using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Models.DTOs.Flag;
using KDomBackend.Services.Interfaces;
using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Notification;

namespace KDomBackend.Services.Implementations
{
    public class FlagService : IFlagService
    {
        private readonly IFlagRepository _repository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IKDomRepository _kdomRepository;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public FlagService(
            IFlagRepository repository,
            IAuditLogRepository auditLogRepository,
            IKDomRepository kdomRepository,
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            IUserService userService,
            INotificationService notificationService)
        {
            _repository = repository;
            _auditLogRepository = auditLogRepository;
            _kdomRepository = kdomRepository;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _userService = userService;
            _notificationService = notificationService;
        }

        public async Task CreateFlagAsync(int userId, FlagCreateDto dto)
        {

            var contentType = (ContentType)dto.ContentType;
            await ValidateContentExistsAsync(contentType, dto.ContentId);

            var flag = new Flag
            {
                UserId = userId,
                ContentType = dto.ContentType, // Rămâne DbEnum pentru stocare
                ContentId = dto.ContentId,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(flag);
        }

        public async Task<List<FlagReadDto>> GetAllAsync()
        {
            var flags = await _repository.GetAllAsync();
            var result = new List<FlagReadDto>();

            foreach (var flag in flags)
            {
                var reporterUsername = await _userService.GetUsernameByUserIdAsync(flag.UserId);

                var contentType = (ContentType)flag.ContentType;
                var content = await LoadFlaggedContentAsync(contentType, flag.ContentId);

                result.Add(new FlagReadDto
                {
                    Id = flag.Id,
                    UserId = flag.UserId,
                    ReporterUsername = reporterUsername,
                    ContentType = flag.ContentType, 
                    ContentId = flag.ContentId,
                    Reason = flag.Reason,
                    CreatedAt = flag.CreatedAt,
                    IsResolved = flag.IsResolved,
                    Content = content.content,
                    ContentExists = content.exists
                });
            }

            return result;
        }


        private async Task<(FlaggedContentDto? content, bool exists)> LoadFlaggedContentAsync(ContentType contentType, string contentId)
        {
            try
            {
                switch (contentType)
                {
                    case ContentType.KDom:
                        var kdom = await _kdomRepository.GetByIdAsync(contentId);
                        if (kdom == null) return (null, false);

                        var kdomAuthor = await _userService.GetUsernameByUserIdAsync(kdom.UserId);
                        return (new FlaggedContentDto
                        {
                            AuthorUsername = kdomAuthor,
                            AuthorId = kdom.UserId,
                            Title = kdom.Title,
                            Text = kdom.ContentHtml,
                            CreatedAt = kdom.CreatedAt
                        }, true);

                    case ContentType.Post:
                        var post = await _postRepository.GetByIdAsync(contentId);
                        if (post == null) return (null, false);

                        var postAuthor = await _userService.GetUsernameByUserIdAsync(post.UserId);
                        return (new FlaggedContentDto
                        {
                            AuthorUsername = postAuthor,
                            AuthorId = post.UserId,
                            Text = post.ContentHtml,
                            CreatedAt = post.CreatedAt,
                            Tags = post.Tags
                        }, true);

                    case ContentType.Comment:
                        var comment = await _commentRepository.GetByIdAsync(contentId);
                        if (comment == null) return (null, false);

                        var commentAuthor = await _userService.GetUsernameByUserIdAsync(comment.UserId);

                        // Încarcă info despre părinte (post sau kdom)
                        string? parentInfo = null;
                        if (comment.TargetType == CommentTargetType.Post)
                        {
                            var parentPost = await _postRepository.GetByIdAsync(comment.TargetId);
                            parentInfo = $"Comment on post: {(parentPost?.ContentHtml.Substring(0, Math.Min(50, parentPost.ContentHtml.Length)))}...";
                        }
                        else if (comment.TargetType == CommentTargetType.KDom)
                        {
                            var parentKdom = await _kdomRepository.GetByIdAsync(comment.TargetId);
                            parentInfo = $"Comment on K-Dom: {parentKdom?.Title}";
                        }

                        return (new FlaggedContentDto
                        {
                            AuthorUsername = commentAuthor,
                            AuthorId = comment.UserId,
                            Text = comment.Text,
                            CreatedAt = comment.CreatedAt,
                            ParentInfo = parentInfo
                        }, true);

                    default:
                        return (null, false);
                }
            }
            catch
            {
                return (null, false);
            }
        }

        public async Task DeleteFlaggedContentAsync(int flagId, int moderatorId, string? moderationReason = null)
        {
            var flag = await _repository.GetFlagByIdAsync(flagId);
            if (flag == null)
                throw new Exception("Flag not found.");


            var contentType = (ContentType)flag.ContentType;
            var content = await LoadFlaggedContentAsync(contentType, flag.ContentId);
            if (!content.exists)
                throw new Exception("Content no longer exists.");

            // Șterge conținutul
            switch (contentType) 
            {
                case ContentType.KDom:
                    await _kdomRepository.DeleteAsync(flag.ContentId);
                    break;
                case ContentType.Post:
                    await _postRepository.DeleteAsync(flag.ContentId);
                    break;
                case ContentType.Comment:
                    await _commentRepository.DeleteAsync(flag.ContentId);
                    break;
            }

            // Marchează flag-ul ca rezolvat
            await _repository.MarkResolvedAsync(flagId);

            // Audit log
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = moderatorId,
                Action = AuditAction.DeleteFlag,
                TargetType = AuditTargetType.Flag,
                TargetId = flagId.ToString(),
                CreatedAt = DateTime.UtcNow,
                Details = $"Deleted flagged {contentType}: {moderationReason ?? "Content violated guidelines"}" 
            });

            // Notificare către autor
            if (content.content != null)
            {
                var contentDescription = contentType switch 
                {
                    ContentType.KDom => content.content.Title,
                    ContentType.Post => content.content.Text.Substring(0, Math.Min(50, content.content.Text.Length)) + "...",
                    ContentType.Comment => content.content.Text.Substring(0, Math.Min(50, content.content.Text.Length)) + "...",
                    _ => "your content"
                };

                await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = content.content.AuthorId,
                    Type = NotificationType.SystemMessage,
                    Message = $"Your {contentType.ToString().ToLower()} \"{contentDescription}\" was removed due to reports. Reason: {moderationReason ?? "Content violated community guidelines."}",
                    TriggeredByUserId = moderatorId,
                    TargetType = contentType, 
                    TargetId = flag.ContentId
                });
            }
        }

        public async Task ResolveAsync(int flagId, int userId)
        {
            await _repository.MarkResolvedAsync(flagId);
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                Action = AuditAction.ResolveFlag,
                TargetType = AuditTargetType.Flag,
                TargetId = flagId.ToString(),
                CreatedAt = DateTime.UtcNow,
                Details = "Flag resolved - content is appropriate"
            });
        }

        public async Task DeleteAsync(int flagId, int userId)
        {
            await _repository.DeleteAsync(flagId);
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                Action = AuditAction.DeleteFlag,
                TargetType = AuditTargetType.Flag,
                TargetId = flagId.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }

        private async Task ValidateContentExistsAsync(ContentType contentType, string contentId)
        {
            switch (contentType)
            {
                case ContentType.KDom:
                    var kdom = await _kdomRepository.GetByIdAsync(contentId);
                    if (kdom == null)
                        throw new Exception("K-Dom not found.");
                    break;

                case ContentType.Post:
                    var post = await _postRepository.GetByIdAsync(contentId);
                    if (post == null)
                        throw new Exception("Post not found.");
                    break;

                case ContentType.Comment:
                    var comment = await _commentRepository.GetByIdAsync(contentId);
                    if (comment == null)
                        throw new Exception("Comment not found.");
                    break;

                default:
                    throw new ArgumentException($"Unsupported content type: {contentType}");
            }
        }
    }
}