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
        Console.WriteLine($"[DEBUG] UserProfileRepository.AddRecentlyViewedKDomAsync: user {userId}, kdom {kdomId}");

        var filter = Builders<UserProfile>.Filter.Eq(p => p.UserId, userId);
        var profile = await _collection.Find(filter).FirstOrDefaultAsync();

        if (profile == null)
        {
            Console.WriteLine($"[DEBUG] No profile found for user {userId}, creating new profile");

            // Creează un profil nou dacă nu există
            profile = new UserProfile
            {
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                RecentlyViewedKDomIds = new List<string>(),
                Nickname = string.Empty,
                AvatarUrl = string.Empty,
                Bio = string.Empty,
                ProfileTheme = KDomBackend.Enums.ProfileTheme.Default
            };

            await _collection.InsertOneAsync(profile);
            Console.WriteLine($"[DEBUG] Created new profile for user {userId}");
        }

        // ✅ FIX: Handle pentru clear operation (dacă kdomId este gol)
        if (string.IsNullOrEmpty(kdomId))
        {
            Console.WriteLine($"[DEBUG] Clearing recently viewed list for user {userId}");
            profile.RecentlyViewedKDomIds.Clear();
        }
        else
        {
            Console.WriteLine($"[DEBUG] Before update - RecentlyViewed count: {profile.RecentlyViewedKDomIds.Count}");
            Console.WriteLine($"[DEBUG] Current list: [{string.Join(", ", profile.RecentlyViewedKDomIds)}]");

            // Elimină K-DOM-ul dacă există deja (pentru a-l muta în față)
            profile.RecentlyViewedKDomIds.Remove(kdomId);

            // Adaugă K-DOM-ul la începutul listei
            profile.RecentlyViewedKDomIds.Insert(0, kdomId);

            // Păstrează doar ultimele 5 K-DOM-uri (înainte era 3)
            const int maxRecentlyViewed = 5;
            if (profile.RecentlyViewedKDomIds.Count > maxRecentlyViewed)
            {
                profile.RecentlyViewedKDomIds = profile.RecentlyViewedKDomIds.Take(maxRecentlyViewed).ToList();
            }

            Console.WriteLine($"[DEBUG] After update - RecentlyViewed count: {profile.RecentlyViewedKDomIds.Count}");
            Console.WriteLine($"[DEBUG] Updated list: [{string.Join(", ", profile.RecentlyViewedKDomIds)}]");
        }

        // Salvează modificările
        await _collection.ReplaceOneAsync(filter, profile);
        Console.WriteLine($"[DEBUG] Profile updated successfully for user {userId}");
    }


    public async Task<List<string>> GetRecentlyViewedKDomIdsAsync(int userId)
    {
        Console.WriteLine($"[DEBUG] UserProfileRepository.GetRecentlyViewedKDomIdsAsync: user {userId}");

        var profile = await _collection
            .Find(p => p.UserId == userId)
            .FirstOrDefaultAsync();

        if (profile == null)
        {
            Console.WriteLine($"[DEBUG] No profile found for user {userId}");
            return new List<string>();
        }

        var result = profile.RecentlyViewedKDomIds ?? new List<string>();
        Console.WriteLine($"[DEBUG] Found {result.Count} recently viewed IDs: [{string.Join(", ", result)}]");

        return result;
    }

}
