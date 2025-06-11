using KDomBackend.Data;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Models.MongoEntities.KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Implementations;
using KDomBackend.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

public class KDomFollowRepository : IKDomFollowRepository
{
    private readonly IMongoCollection<KDomFollow> _collection;
    private readonly IKDomRepository _kdomRepository;

    public KDomFollowRepository(MongoDbContext context, IKDomRepository kdomRepository)
    {
        _collection = context.KDomFollows;
        _kdomRepository = kdomRepository;
    }

    public async Task<bool> ExistsAsync(int userId, string kdomId)
    {
        var filter = Builders<KDomFollow>.Filter.And(
            Builders<KDomFollow>.Filter.Eq(f => f.UserId, userId),
            Builders<KDomFollow>.Filter.Eq(f => f.KDomId, kdomId)
        );

        return await _collection.Find(filter).AnyAsync();
    }

    public async Task CreateAsync(KDomFollow follow)
    {
        await _collection.InsertOneAsync(follow);
    }

    public async Task UnfollowAsync(int userId, string kdomId)
    {
        var filter = Builders<KDomFollow>.Filter.And(
            Builders<KDomFollow>.Filter.Eq(f => f.UserId, userId),
            Builders<KDomFollow>.Filter.Eq(f => f.KDomId, kdomId)
        );

        await _collection.DeleteOneAsync(filter);
    }
    public async Task<List<string>> GetFollowedKDomIdsAsync(int userId)
    {
        var filter = Builders<KDomFollow>.Filter.Eq(f => f.UserId, userId);
        var result = await _collection.Find(filter).ToListAsync();
        return result.Select(f => f.KDomId).ToList();
    }

    public async Task<Dictionary<string, int>> CountRecentFollowsAsync(int days = 7)
    {
        var fromDate = DateTime.UtcNow.AddDays(-days);

        var pipeline = new[]
        {
        new BsonDocument("$match", new BsonDocument("followedAt",
            new BsonDocument("$gte", fromDate))),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$kdomId" },
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

    public async Task<List<string>> GetFollowedSlugsAsync(int userId)
    {
        var followedIds = await GetFollowedKDomIdsAsync(userId);
        var kdoms = await _kdomRepository.GetByIdsAsync(followedIds);
        return kdoms.Select(k => k.Slug).ToList();
    }

    public async Task<int> GetFollowersCountAsync(string kdomId)
    {
        var filter = Builders<KDomFollow>.Filter.Eq(f => f.KDomId, kdomId);
        return (int)await _collection.CountDocumentsAsync(filter);
    }


}
