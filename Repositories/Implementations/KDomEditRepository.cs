using MongoDB.Driver;
using MongoDB.Bson;
using KDomBackend.Data;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;

public class KDomEditRepository : IKDomEditRepository
{
    private readonly IMongoCollection<KDomEdit> _collection;

    public KDomEditRepository(MongoDbContext context)
    {
        _collection = context.KDomEdits;
    }

    public async Task<List<string>> GetEditedKDomIdsByUserAsync(int userId, int days = 30)
    {
        var fromDate = DateTime.UtcNow.AddDays(-days);

        var filter = Builders<KDomEdit>.Filter.And(
            Builders<KDomEdit>.Filter.Eq(e => e.UserId, userId),
            Builders<KDomEdit>.Filter.Gte(e => e.EditedAt, fromDate)
        );

        var edits = await _collection.Find(filter).ToListAsync();
        return edits.Select(e => e.KDomId).Distinct().ToList();
    }

    public async Task<Dictionary<string, int>> CountRecentEditsAsync(int days = 7)
    {
        var fromDate = DateTime.UtcNow.AddDays(-days);

        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument("editedAt", new BsonDocument("$gte", fromDate))),
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
}
