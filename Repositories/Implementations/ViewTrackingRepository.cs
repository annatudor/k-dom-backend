using KDomBackend.Data;
using KDomBackend.Enums;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KDomBackend.Repositories.Implementations
{
    public class ViewTrackingRepository : IViewTrackingRepository
    {
        private readonly IMongoCollection<ViewTracking> _collection;

        public ViewTrackingRepository(MongoDbContext context)
        {
            _collection = context.ViewTrackings;
        }

        public async Task CreateAsync(ViewTracking viewTracking)
        {
            await _collection.InsertOneAsync(viewTracking);
        }

        public async Task<int> GetViewCountAsync(ContentType contentType, string contentId)
        {
            var filter = Builders<ViewTracking>.Filter.And(
                Builders<ViewTracking>.Filter.Eq(v => v.ContentType, contentType),
                Builders<ViewTracking>.Filter.Eq(v => v.ContentId, contentId)
            );

            return (int)await _collection.CountDocumentsAsync(filter);
        }

        public async Task<int> GetUserContentViewsAsync(int userId, ContentType contentType)
        {
            // Pentru a calcula views pe conținutul unui user, trebuie să facem join
            // cu repository-urile respective (Post/KDom) pentru a găsi conținutul user-ului
            // Pentru simplitate, returnăm 0 și implementăm în service prin agregare
            return 0;
        }

        public async Task<Dictionary<string, int>> GetUserContentViewsBreakdownAsync(int userId, ContentType contentType)
        {
            // Similar, implementare în service
            return new Dictionary<string, int>();
        }

        public async Task<bool> HasRecentViewAsync(ContentType contentType, string contentId, int? userId, string? ipAddress, int minutesWindow = 30)
        {
            var fromTime = DateTime.UtcNow.AddMinutes(-minutesWindow);

            var filterBuilder = Builders<ViewTracking>.Filter.And(
                Builders<ViewTracking>.Filter.Eq(v => v.ContentType, contentType),
                Builders<ViewTracking>.Filter.Eq(v => v.ContentId, contentId),
                Builders<ViewTracking>.Filter.Gte(v => v.ViewedAt, fromTime)
            );

            // Verifică după user ID dacă este logat, altfel după IP
            if (userId.HasValue)
            {
                filterBuilder = Builders<ViewTracking>.Filter.And(
                    filterBuilder,
                    Builders<ViewTracking>.Filter.Eq(v => v.ViewerId, userId.Value)
                );
            }
            else if (!string.IsNullOrEmpty(ipAddress))
            {
                filterBuilder = Builders<ViewTracking>.Filter.And(
                    filterBuilder,
                    Builders<ViewTracking>.Filter.Eq(v => v.IpAddress, ipAddress)
                );
            }

            return await _collection.Find(filterBuilder).AnyAsync();
        }

        public async Task<Dictionary<string, int>> GetTopViewedContentAsync(ContentType contentType, int limit = 10)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("contentType", contentType.ToString())),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$contentId" },
                    { "viewCount", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("viewCount", -1)),
                new BsonDocument("$limit", limit)
            };

            var cursor = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var results = await cursor.ToListAsync();

            return results.ToDictionary(
                doc => doc["_id"].AsString,
                doc => doc["viewCount"].AsInt32
            );
        }

        public async Task<List<ViewTracking>> GetRecentViewsAsync(int? userId, int limit = 20)
        {
            var filter = userId.HasValue
                ? Builders<ViewTracking>.Filter.Eq(v => v.ViewerId, userId.Value)
                : Builders<ViewTracking>.Filter.Empty;

            return await _collection
                .Find(filter)
                .SortByDescending(v => v.ViewedAt)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<int> GetTotalViewsByUserAsync(int userId)
        {
            // Calculează views pe tot conținutul unui user
            // Va fi implementat în service prin agregare cu alte repository-uri
            return 0;
        }
    }
}