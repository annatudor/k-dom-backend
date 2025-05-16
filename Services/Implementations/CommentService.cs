using KDomBackend.Enums;
using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.Comment;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repository;
        private readonly IUserService _userService;

        public CommentService(ICommentRepository repository, IUserService userService)
        {
            _repository = repository;
            _userService = userService;
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
        }

        public async Task<List<CommentReadDto>> GetCommentsByTargetAsync(CommentTargetType type, string targetId)
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
                    EditedAt = comment.EditedAt
                });
            }

            return result;
        }
        public async Task<List<CommentReadDto>> GetRepliesAsync(string parentCommentId)
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
                    EditedAt = comment.EditedAt
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
        public async Task DeleteCommentAsync(string commentId, int userId)
        {
            var comment = await _repository.GetByIdAsync(commentId);
            if (comment == null)
                throw new Exception("Comment not found.");

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to delete this comment.");

            await _repository.DeleteAsync(commentId);
        }


    }
}
