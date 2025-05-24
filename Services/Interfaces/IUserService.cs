using KDomBackend.Models.DTOs.Common;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.Entities;

namespace KDomBackend.Services.Interfaces
{
    public interface IUserService
    {
        Task<int> RegisterUserAsync(UserRegisterDto dto);
        Task<string> AuthenticateAsync(UserLoginDto dto);
        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task RequestPasswordResetAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task<string> GetUsernameByUserIdAsync(int userId);
        Task<UserProfileReadDto> GetUserProfileAsync(int userId);
        Task UpdateProfileAsync(int userId, UserProfileUpdateDto dto);
        Task ChangeUserRoleAsync(int targetUserId, string newRole, int adminUserId);
        Task<PagedResult<UserPublicDto>> GetAllPaginatedAsync(UserFilterDto filter);
        Task<User?> GetUserByUsernameAsync(string username);
        Task AddRecentlyViewedKDomAsync(int userId, string kdomId);
        Task<List<string>> GetRecentlyViewedKDomIdsAsync(int userId);


    }
}
