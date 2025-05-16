using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Comment;

namespace KDomBackend.Services.Interfaces
{
    public interface ICommentService
    {
        Task CreateCommentAsync(CommentCreateDto dto, int userId);
        Task<List<CommentReadDto>> GetCommentsByTargetAsync(CommentTargetType type, string targetId);
        Task<List<CommentReadDto>> GetRepliesAsync(string parentCommentId);
        Task EditCommentAsync(string commentId, CommentEditDto dto, int userId);
        Task DeleteCommentAsync(string commentId, int userId);

    }
}
