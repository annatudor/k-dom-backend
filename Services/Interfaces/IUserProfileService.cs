using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.Entities;

namespace KDomBackend.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileReadDto> GetUserProfileAsync(int userId);
        Task UpdateProfileAsync(int userId, UserProfileUpdateDto dto);
        Task AddRecentlyViewedKDomAsync(int userId, string kdomId);
        Task<List<string>> GetRecentlyViewedKDomIdsAsync(int userId);
    }
}
