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

    public async Task AddRecentlyViewedKDomAsync(int userId, string kdomId)
    {
        var filter = Builders<UserProfile>.Filter.Eq(p => p.UserId, userId);
        var profile = await _collection.Find(filter).FirstOrDefaultAsync();

        if (profile == null) return;

        profile.RecentlyViewedKDomIds.Remove(kdomId); // eliminam daca exista deja
        profile.RecentlyViewedKDomIds.Insert(0, kdomId); // adăugăm in fata

        if (profile.RecentlyViewedKDomIds.Count > 3)
            profile.RecentlyViewedKDomIds = profile.RecentlyViewedKDomIds.Take(3).ToList();

        await _collection.ReplaceOneAsync(filter, profile);
    }

    public async Task<List<string>> GetRecentlyViewedKDomIdsAsync(int userId)
    {
        var profile = await _collection
            .Find(p => p.UserId == userId)
            .FirstOrDefaultAsync();

        return profile?.RecentlyViewedKDomIds ?? new();
    }

}
