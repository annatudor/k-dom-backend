using KDomBackend.Models.DTOs.User;

namespace KDomBackend.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileReadDto> GetUserProfileAsync(int userId, int? viewerUserId = null);
        Task UpdateProfileAsync(int userId, UserProfileUpdateDto dto);
        Task AddRecentlyViewedKDomAsync(int userId, string kdomId);
        Task<List<string>> GetRecentlyViewedKDomIdsAsync(int userId);
        Task<bool> CanUserUpdateProfileAsync(int currentUserId, int targetUserId);
        Task ValidateUpdatePermissionsAsync(int currentUserId, int targetUserId);
        Task<UserPrivateInfoDto> GetUserPrivateInfoAsync(int userId, int requesterId);
        Task<UserDetailedStatsDto> GetUserDetailedStatsAsync(int userId, int requesterId);
        Task<bool> IsUserAdminAsync(int userId);
        Task<bool> IsUserAdminOrModeratorAsync(int userId);

    }
}