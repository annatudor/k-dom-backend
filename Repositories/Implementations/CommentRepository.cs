using KDomBackend.Data;
using KDomBackend.Enums;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using MongoDB.Driver;

namespace KDomBackend.Repositories.Implementations
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IMongoCollection<Comment> _collection;

        public CommentRepository(MongoDbContext context)
        {
            _collection = context.Comments;
        }

        public async Task CreateAsync(Comment comment)
        {
            await _collection.InsertOneAsync(comment);
        }

        public async Task<List<Comment>> GetByTargetAsync(CommentTargetType type, string targetId)
        {
            return await _collection
                .Find(c => c.TargetType == type && c.TargetId == targetId)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetRepliesAsync(string parentCommentId)
        {
            return await _collection
                .Find(c => c.ParentCommentId == parentCommentId)
                .SortBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(string id)
        {
            return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateTextAsync(string id, string newText)
        {
            var update = Builders<Comment>.Update
                .Set(c => c.Text, newText)
                .Set(c => c.IsEdited, true)
                .Set(c => c.EditedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(c => c.Id == id, update);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(c => c.Id == id);
        }

        public async Task ToggleLikeAsync(string commentId, int userId, bool like)
        {
            var update = like
                ? Builders<Comment>.Update.AddToSet(c => c.Likes, userId)
                : Builders<Comment>.Update.Pull(c => c.Likes, userId);

            await _collection.UpdateOneAsync(c => c.Id == commentId, update);
        }


    }
}
