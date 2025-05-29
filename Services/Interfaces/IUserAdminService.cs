using KDomBackend.Models.DTOs.Common;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.Entities;

namespace KDomBackend.Services.Interfaces
{
    public interface IUserAdminService
    {
        Task ChangeUserRoleAsync(int targetUserId, string newRole, int adminUserId);
        Task<PagedResult<UserPublicDto>> GetAllPaginatedAsync(UserFilterDto filter);
        
    }
}
