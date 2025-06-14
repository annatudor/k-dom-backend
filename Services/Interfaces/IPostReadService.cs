﻿using KDomBackend.Models.DTOs.Common;
using KDomBackend.Models.DTOs.Post;

namespace KDomBackend.Services.Interfaces
{
    public interface IPostReadService
    {
        Task<List<PostReadDto>> GetAllPostsAsync();
        Task<PostReadDto> GetByIdAsync(string postId);
        Task<List<PostReadDto>> GetPostsByUserIdAsync(int userId);
        Task<List<PostReadDto>> GetFeedAsync(int userId);
        Task<List<PostReadDto>> GetGuestFeedAsync(int limit = 30);
        Task<List<PostReadDto>> GetPostsByTagAsync(string tag);
        Task<PagedResult<PostReadDto>> GetPostsByTagAsync(string tag, int page, int pageSize);
        Task<int> GetPostsCountByTagAsync(string tag);
    }
}
