using KDomBackend.Data;
using KDomBackend.Enums;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using MongoDB.Bson;
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

        public async Task<Dictionary<string, int>> CountRecentCommentsByKDomAsync(int days = 7)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var filter = Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(c => c.TargetType, CommentTargetType.KDom),
                Builders<Comment>.Filter.Gte(c => c.CreatedAt, fromDate)
            );

            var pipeline = new[]
            {
        new BsonDocument("$match", new BsonDocument {
            { "targetType", "KDom" },
            { "createdAt", new BsonDocument("$gte", fromDate) }
        }),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$targetId" },
            { "count", new BsonDocument("$sum", 1) }
        })
    };

            var cursor = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var list = await cursor.ToListAsync();

            return list.ToDictionary(
                doc => doc["_id"].AsString,
                doc => doc["count"].AsInt32
            );
        }

        public async Task<List<string>> GetCommentedKDomIdsByUserAsync(int userId, int days = 30)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var filter = Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(c => c.UserId, userId),
                Builders<Comment>.Filter.Eq(c => c.TargetType, CommentTargetType.KDom),
                Builders<Comment>.Filter.Gte(c => c.CreatedAt, fromDate)
            );

            var comments = await _collection.Find(filter).ToListAsync();

            return comments.Select(c => c.TargetId).Distinct().ToList();
        }


    }
}
