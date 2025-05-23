﻿using KDomBackend.Enums;
using KDomBackend.Models.MongoEntities;

namespace KDomBackend.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task CreateAsync(Comment comment);
        Task<List<Comment>> GetByTargetAsync(CommentTargetType type, string targetId);
        Task<List<Comment>> GetRepliesAsync(string parentCommentId);
        Task<Comment?> GetByIdAsync(string id);
        Task UpdateTextAsync(string id, string newText);
        Task DeleteAsync(string id);
        Task ToggleLikeAsync(string commentId, int userId, bool like);
        Task<Dictionary<string, int>> CountRecentCommentsByKDomAsync(int days = 7);
        Task<List<string>> GetCommentedKDomIdsByUserAsync(int userId, int days = 30);


    }
}
