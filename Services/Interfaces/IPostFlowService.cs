using KDomBackend.Models.DTOs.Post;

namespace KDomBackend.Services.Interfaces
{
    public interface IPostFlowService
    {
        Task CreatePostAsync(PostCreateDto dto, int userId);
        Task<PostLikeResponseDto> ToggleLikeAsync(string postId, int userId);
        Task EditPostAsync(string postId, PostEditDto dto, int userId);
        Task DeletePostAsync(string postId, int userId, bool isModerator);
    }
}
