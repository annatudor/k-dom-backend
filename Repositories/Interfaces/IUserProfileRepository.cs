using KDomBackend.Models.MongoEntities;

namespace KDomBackend.Repositories.Interfaces
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetProfileByUserIdAsync(int userId);
        Task CreateAsync(UserProfile profile);
        Task UpdateAsync(UserProfile profile);

    }
}
