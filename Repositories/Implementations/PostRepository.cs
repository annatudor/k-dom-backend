using KDomBackend.Data;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
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

    }
}
