using KDomBackend.Data;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using MongoDB.Driver;

namespace KDomBackend.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notification> _collection;

        public NotificationRepository(MongoDbContext context)
        {
            _collection = context.Notifications;
        }

        public async Task CreateAsync(Notification notification)
        {
            await _collection.InsertOneAsync(notification);
        }

        public async Task<List<Notification>> GetByUserIdAsync(int userId)
        {
            return await _collection
                .Find(n => n.UserId == userId)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(string id)
        {
            return await _collection.Find(n => n.Id == id).FirstOrDefaultAsync();
        }

        public async Task MarkAsReadAsync(string id)
        {
            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
            await _collection.UpdateOneAsync(n => n.Id == id, update);
        }

        public async Task<List<Notification>> GetUnreadByUserIdAsync(int userId)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq(n => n.IsRead, false)
            );

            return await _collection
                .Find(filter)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountUnreadAsync(int userId)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq(n => n.IsRead, false)
            );

            var count = await _collection.CountDocumentsAsync(filter);
            return (int)count;
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq(n => n.IsRead, false)
            );

            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
            await _collection.UpdateManyAsync(filter, update);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(n => n.Id == id);
        }


    }
}
