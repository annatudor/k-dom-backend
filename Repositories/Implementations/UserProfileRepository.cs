using KDomBackend.Data;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using MongoDB.Driver;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly IMongoCollection<UserProfile> _collection;

    public UserProfileRepository(MongoDbContext context)
    {
        _collection = context.UserProfiles;
    }

    public async Task<UserProfile?> GetProfileByUserIdAsync(int userId)
    {
        return await _collection
            .Find(p => p.UserId == userId)
            .FirstOrDefaultAsync();
    }
    public async Task CreateAsync(UserProfile profile)
    {
        await _collection.InsertOneAsync(profile);
    }

    public async Task UpdateAsync(UserProfile profile)
    {
        var filter = Builders<UserProfile>.Filter.Eq(p => p.UserId, profile.UserId);
        await _collection.ReplaceOneAsync(filter, profile);
    }

}
