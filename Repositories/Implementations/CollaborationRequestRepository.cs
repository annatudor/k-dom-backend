using KDomBackend.Data;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using MongoDB.Driver;
using KDomBackend.Enums;

public class CollaborationRequestRepository : ICollaborationRequestRepository
{
    private readonly IMongoCollection<KDomCollaborationRequest> _collection;

    public CollaborationRequestRepository(MongoDbContext context)
    {
        _collection = context.KDomCollaborationRequests;
    }

    public async Task CreateAsync(KDomCollaborationRequest request)
    {
        await _collection.InsertOneAsync(request);
    }

    public async Task<bool> HasPendingAsync(string kdomId, int userId)
    {
        var filter = Builders<KDomCollaborationRequest>.Filter.And(
            Builders<KDomCollaborationRequest>.Filter.Eq(r => r.KDomId, kdomId),
            Builders<KDomCollaborationRequest>.Filter.Eq(r => r.UserId, userId),
            Builders<KDomCollaborationRequest>.Filter.Eq(r => r.Status, CollaborationRequestStatus.Pending)
        );

        return await _collection.Find(filter).AnyAsync();
    }

    public async Task<KDomCollaborationRequest?> GetByIdAsync(string requestId)
    {
        return await _collection.Find(r => r.Id == requestId).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(KDomCollaborationRequest request)
    {
        await _collection.ReplaceOneAsync(r => r.Id == request.Id, request);
    }

    public async Task<List<KDomCollaborationRequest>> GetByKDomIdAsync(string kdomId)
    {
        var filter = Builders<KDomCollaborationRequest>.Filter.Eq(r => r.KDomId, kdomId);
        return await _collection.Find(filter).SortByDescending(r => r.CreatedAt).ToListAsync();
    }


}
