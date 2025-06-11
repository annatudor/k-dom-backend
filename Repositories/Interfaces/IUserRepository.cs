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


        // Statistici pentru activitate și contribuții
        Task<int> GetUserKDomsCreatedCountAsync(int userId);
        Task<int> GetUserKDomsCollaboratedCountAsync(int userId);
        Task<int> GetUserPostsCountAsync(int userId);
        Task<int> GetUserCommentsCountAsync(int userId);
        Task<DateTime?> GetUserLastActivityAsync(int userId);

        // Statistici detaliate pentru admin
        Task<int> GetUserTotalLikesReceivedAsync(int userId);
        Task<int> GetUserTotalLikesGivenAsync(int userId);
        Task<int> GetUserCommentsReceivedAsync(int userId);
        Task<int> GetUserFlagsReceivedAsync(int userId);
        Task<Dictionary<string, int>> GetUserActivityByMonthAsync(int userId, int months = 12);
        Task<List<string>> GetUserRecentActionsAsync(int userId, int limit = 10);

        Task UpdateLastLoginAsync(int userId, DateTime loginTime);
        Task<DateTime?> GetLastLoginAsync(int userId);

        Task<User?> GetByUsernameOrEmailAsync(string identifier);
        Task<int> GetTotalUsersCountAsync();

    }
}
