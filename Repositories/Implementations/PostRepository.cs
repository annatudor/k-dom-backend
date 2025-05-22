using KDomBackend.Data;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KDomBackend.Repositories.Implementations
{
    public class PostRepository : IPostRepository
    {
        private readonly IMongoCollection<Post> _collection;

        public PostRepository(MongoDbContext context)
        {
            _collection = context.Posts;
        }

        public async Task CreateAsync(Post post)
        {
            await _collection.InsertOneAsync(post);
        }

        public async Task<List<Post>> GetAllAsync()
        {
            return await _collection
                .Find(_ => true)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Post?> GetByIdAsync(string id)
        {
            return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task ToggleLikeAsync(string postId, int userId, bool like)
        {
            var update = like
                ? Builders<Post>.Update.AddToSet(p => p.Likes, userId)
                : Builders<Post>.Update.Pull(p => p.Likes, userId);

            await _collection.UpdateOneAsync(p => p.Id == postId, update);
        }

        public async Task UpdateAsync(string postId, string newHtml, List<string> newTags)
        {
            var update = Builders<Post>.Update
                .Set(p => p.ContentHtml, newHtml)
                .Set(p => p.Tags, newTags)
                .Set(p => p.IsEdited, true)
                .Set(p => p.EditedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(p => p.Id == postId, update);
        }
        public async Task DeleteAsync(string postId)
        {
            await _collection.DeleteOneAsync(p => p.Id == postId);
        }

        public async Task<List<Post>> GetByUserIdAsync(int userId)
        {
            return await _collection
                .Find(p => p.UserId == userId)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetFeedPostsAsync(List<int> followedUserIds, List<string> followedTags, int limit = 30)
        {
            var filterUser = Builders<Post>.Filter.In(p => p.UserId, followedUserIds);
            var filterTag = Builders<Post>.Filter.AnyIn(p => p.Tags, followedTags);

            var combinedFilter = Builders<Post>.Filter.Or(filterUser, filterTag);

            return await _collection.Find(combinedFilter)
                .SortByDescending(p => p.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }


        public async Task<List<Post>> GetPublicPostsAsync(int limit = 30)
        {
            return await _collection
                .Find(_ => true)
                .SortByDescending(p => p.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }
        public async Task<List<Post>> GetByTagAsync(string tag)
        {
            var filter = Builders<Post>.Filter.AnyEq(p => p.Tags, tag.ToLower());
            return await _collection.Find(filter)
                                    .SortByDescending(p => p.CreatedAt)
                                    .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetRecentTagCountsAsync(int days = 7)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);

            var pipeline = new[]
            {
        new BsonDocument("$match", new BsonDocument("createdAt", new BsonDocument("$gte", fromDate))),
        new BsonDocument("$unwind", "$tags"),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$tags" },
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

        public async Task<List<Post>> GetRecentPostsByUserAsync(int userId, int limit = 30)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.UserId, userId);

            return await _collection
                .Find(filter)
                .SortByDescending(p => p.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }


    }
}
