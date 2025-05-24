using KDomBackend.Models.Entities;

namespace KDomBackend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<string> GetUsernameByUserIdAsync(int userId);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int id);
        Task<int> CreateAsync(User user);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task UpdatePasswordAsync(int userId, string newHashedPassword);
        Task<User?> GetByProviderIdAsync(string provider, string providerId);
        Task UpdateRoleAsync(int userId, string newRole);
        Task<List<User>> GetPaginatedAsync(int skip, int take, string? role = null, string? search = null);
        Task<int> CountAsync(string? role = null, string? search = null);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<List<User>> GetUsersByRolesAsync(string[] roles);
        Task<List<User>> SearchUsersAsync(string query);


    }
}
