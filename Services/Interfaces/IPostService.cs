using KDomBackend.Models.DTOs.Post;

namespace KDomBackend.Services.Interfaces
{
    public interface IPostService
    {
        Task CreatePostAsync(PostCreateDto dto, int userId);
        Task<List<PostReadDto>> GetAllPostsAsync();
        Task<PostReadDto> GetByIdAsync(string postId);
        Task<PostLikeResponseDto> ToggleLikeAsync(string postId, int userId);
        Task EditPostAsync(string postId, PostEditDto dto, int userId);
        Task DeletePostAsync(string postId, int userId);
        Task<List<PostReadDto>> GetPostsByUserIdAsync(int userId);

    }
}
