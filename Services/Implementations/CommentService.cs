using KDomBackend.Enums;
using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.Comment;
using KDomBackend.Models.DTOs.Notification;
using KDomBackend.Models.Entities;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repository;
        private readonly IUserService _userService;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly INotificationService _notificationService;

        public CommentService(ICommentRepository repository, IUserService userService, IAuditLogRepository auditLogRepository,
        INotificationService notificationService)
        {
            _repository = repository;
            _userService = userService;
            _auditLogRepository = auditLogRepository;
            _notificationService = notificationService;
        }

        public async Task CreateCommentAsync(CommentCreateDto dto, int userId)
        {
            var cleanText = HtmlSanitizerHelper.Sanitize(dto.Text);

            var comment = new Comment
            {
                TargetType = dto.TargetType,
                TargetId = dto.TargetId,
                UserId = userId,
                Text = cleanText,
                ParentCommentId = dto.ParentCommentId,
                CreatedAt = DateTime.UtcNow,
                IsEdited = false
            };

            await _repository.CreateAsync(comment);

            if (!string.IsNullOrEmpty(comment.ParentCommentId))
            {
                var parent = await _repository.GetByIdAsync(comment.ParentCommentId);
                if (parent != null && parent.UserId != userId)
                {
                    var triggeredByUsername = await _userService.GetUsernameByUserIdAsync(userId);

                    await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                    {
                        UserId = parent.UserId,
                        Type = NotificationType.CommentReply,
                        Message = $"{triggeredByUsername} replied to your comment.",
                        TriggeredByUserId = userId,
                        TargetType = ContentType.Comment,
                        TargetId = comment.Id
                    });
                }
            }

            await MentionHelper.HandleMentionsAsync(
                    dto.Text,
                    userId,
                    comment.Id,
                    ContentType.Comment,
                    NotificationType.MentionInComment,
                    _userService,
                    _notificationService);


        }

        public async Task<List<CommentReadDto>> GetCommentsByTargetAsync(CommentTargetType type, string targetId, int? currentUserId = null)
        {
            var comments = await _repository.GetByTargetAsync(type, targetId);
            var result = new List<CommentReadDto>();

            foreach (var comment in comments)
            {
                var username = await _userService.GetUsernameByUserIdAsync(comment.UserId);

                result.Add(new CommentReadDto
                {
                    Id = comment.Id,
                    TargetType = comment.TargetType,
                    TargetId = comment.TargetId,
                    UserId = comment.UserId,
                    Username = username ?? "unknown",
                    Text = comment.Text,
                    ParentCommentId = comment.ParentCommentId,
                    CreatedAt = comment.CreatedAt,
                    IsEdited = comment.IsEdited,
                    EditedAt = comment.EditedAt,
                    Likes = comment.Likes,
                    LikeCount = comment.Likes.Count,
                    IsLikedByUser = currentUserId.HasValue && comment.Likes.Contains(currentUserId.Value)
                });
            }

            return result;
        }

        public async Task<List<CommentReadDto>> GetRepliesAsync(string parentCommentId, int? currentUserId = null)
        {
            var replies = await _repository.GetRepliesAsync(parentCommentId);
            var result = new List<CommentReadDto>();

            foreach (var comment in replies)
            {
                var username = await _userService.GetUsernameByUserIdAsync(comment.UserId);

                result.Add(new CommentReadDto
                {
                    Id = comment.Id,
                    TargetType = comment.TargetType,
                    TargetId = comment.TargetId,
                    UserId = comment.UserId,
                    Username = username ?? "unknown",
                    Text = comment.Text,
                    ParentCommentId = comment.ParentCommentId,
                    CreatedAt = comment.CreatedAt,
                    IsEdited = comment.IsEdited,
                    EditedAt = comment.EditedAt,
                    Likes = comment.Likes,
                    LikeCount = comment.Likes.Count,
                    IsLikedByUser = currentUserId.HasValue && comment.Likes.Contains(currentUserId.Value)
                });
            }

            return result;
        }

        public async Task EditCommentAsync(string commentId, CommentEditDto dto, int userId)
        {
            var comment = await _repository.GetByIdAsync(commentId);
            if (comment == null)
                throw new Exception("Comment not found.");

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You are not the owner of this comment.");

            var cleanText = HtmlSanitizerHelper.Sanitize(dto.Text);

            if (cleanText == comment.Text)
                return; // nu s-a modificat

            await _repository.UpdateTextAsync(commentId, cleanText);
        }
        public async Task DeleteCommentAsync(string commentId, int userId, bool isModerator)
        {
            var comment = await _repository.GetByIdAsync(commentId);
            if (comment == null)
                throw new Exception("Comment not found.");

            var isOwner = comment.UserId == userId;

            if (!isOwner && !isModerator)
                throw new UnauthorizedAccessException("You are not allowed to delete this comment.");

            await _repository.DeleteAsync(commentId);

            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                Action = AuditAction.DeleteComment,
                TargetType = AuditTargetType.Comment,
                TargetId = commentId,
                CreatedAt = DateTime.UtcNow,
                Details = isModerator ? "Deleted by moderator" : "Deleted by autor"
            });

            if (isModerator)
            {
                await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = comment.UserId,
                    Type = NotificationType.SystemMessage,
                    Message = "Your comment has been deleted by a moderator.",
                    TriggeredByUserId = userId,
                    TargetType = ContentType.Comment,
                    TargetId = commentId
                });
            }
        }

        public async Task<CommentLikeResponseDto> ToggleLikeAsync(string commentId, int userId)
        {
            var comment = await _repository.GetByIdAsync(commentId);
            if (comment == null)
                throw new Exception("Comment not found.");

            var alreadyLiked = comment.Likes.Contains(userId);
            var like = !alreadyLiked;

            await _repository.ToggleLikeAsync(commentId, userId, like);

            // Calculează count-ul direct în loc să facă un query suplimentar
            var newCount = like ? comment.Likes.Count + 1 : comment.Likes.Count - 1;

            // Notificare doar când se adaugă like (nu când se elimină)
            if (like && comment.UserId != userId)
            {
                var likerUsername = await _userService.GetUsernameByUserIdAsync(userId);
                await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = comment.UserId,
                    Type = NotificationType.CommentLiked,
                    Message = $"{likerUsername} liked your comment.",
                    TriggeredByUserId = userId,
                    TargetType = ContentType.Comment,
                    TargetId = comment.Id
                });
            }

            return new CommentLikeResponseDto
            {
                Liked = like,
                LikeCount = newCount
            };
        }


    }
}
