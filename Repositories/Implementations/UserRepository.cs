using Dapper;
using System.Data;
using KDomBackend.Data;
using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;

namespace KDomBackend.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _context;

        public UserRepository(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<string> GetUsernameByUserIdAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            return user?.Username ?? "deleted user";
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"
        SELECT 
            id,
            username,
            email,
            password_hash AS PasswordHash,
            provider,
            provider_id,
            role_id,
            created_at,
            is_active
        FROM users
        WHERE email = @Email";

            var user = await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
            return user;
        }



        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"
        SELECT 
            id,
            username,
            email,
            password_hash AS PasswordHash,
            provider,
            provider_id,
            role_id,
            created_at,
            is_active
        FROM users WHERE username = @Username";

            var user = await conn.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });

            Console.WriteLine($"[DEBUG] user is null: {user == null}");
            Console.WriteLine($"[DEBUG] user.PasswordHash: {user?.PasswordHash}");

            return user;
        }


        public async Task<User?> GetByIdAsync(int id)
        {
            using var conn = _context.CreateConnection();
            const string sql = "SELECT * FROM users WHERE id = @Id";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(User user)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"
                INSERT INTO users (username, email, password_hash, provider, provider_id, role_id, created_at, is_active)
                VALUES (@Username, @Email, @PasswordHash, @Provider, @ProviderId, @RoleId, @CreatedAt, @IsActive);
                SELECT LAST_INSERT_ID();";
            return await conn.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            using var conn = _context.CreateConnection();
            const string sql = "SELECT COUNT(1) FROM users WHERE email = @Email";
            return await conn.ExecuteScalarAsync<bool>(sql, new { Email = email });
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            using var conn = _context.CreateConnection();
            const string sql = "SELECT COUNT(1) FROM users WHERE username = @Username";
            return await conn.ExecuteScalarAsync<bool>(sql, new { Username = username });
        }

        public async Task UpdatePasswordAsync(int userId, string newHashedPassword)
        {
            using var conn = _context.CreateConnection();
            const string sql = "UPDATE users SET password_hash = @PasswordHash WHERE id = @UserId";
            await conn.ExecuteAsync(sql, new { PasswordHash = newHashedPassword, UserId = userId });
        }

        public async Task<User?> GetByProviderIdAsync(string provider, string providerId)
        {
            using var conn = _context.CreateConnection();

            const string sql = @"
        SELECT 
            id, username, email, password_hash AS PasswordHash,
            provider, provider_id, role_id, created_at, is_active
        FROM users
        WHERE provider = @Provider AND provider_id = @ProviderId";

            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Provider = provider, ProviderId = providerId });
        }

        public async Task UpdateRoleAsync(int userId, string newRole)
        {
            using var conn = _context.CreateConnection();
            const string sql = "UPDATE users SET role_id = (SELECT id FROM roles WHERE name = @RoleName) WHERE id = @UserId";
            await conn.ExecuteAsync(sql, new { RoleName = newRole.ToLower(), UserId = userId });
        }

        public async Task<List<User>> GetPaginatedAsync(int skip, int take, string? role = null, string? search = null)
        {
            using var conn = _context.CreateConnection();

            var sql = @"
        SELECT u.*, r.name AS Role
        FROM users u
        JOIN roles r ON u.role_id = r.id
        WHERE 1 = 1";

            if (!string.IsNullOrEmpty(role))
                sql += " AND r.name = @Role";

            if (!string.IsNullOrEmpty(search))
                sql += " AND (u.username LIKE @Search OR u.email LIKE @Search)";

            sql += " ORDER BY u.created_at DESC LIMIT @Take OFFSET @Skip";

            return (await conn.QueryAsync<User>(sql, new
            {
                Role = role?.ToLower(),
                Search = $"%{search}%",
                Take = take,
                Skip = skip
            })).ToList();
        }

        public async Task<int> CountAsync(string? role = null, string? search = null)
        {
            using var conn = _context.CreateConnection();

            var sql = @"
        SELECT COUNT(*)
        FROM users u
        JOIN roles r ON u.role_id = r.id
        WHERE 1 = 1";

            if (!string.IsNullOrEmpty(role))
                sql += " AND r.name = @Role";

            if (!string.IsNullOrEmpty(search))
                sql += " AND (u.username LIKE @Search OR u.email LIKE @Search)";

            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                Role = role?.ToLower(),
                Search = $"%{search}%"
            });
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            using var conn = _context.CreateConnection();
            const string sql = "SELECT * FROM users WHERE username = @Username";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<List<User>> GetUsersByRolesAsync(string[] roles)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"
        SELECT u.*, r.name AS Role
        FROM users u
        JOIN roles r ON u.role_id = r.id
        WHERE r.name IN @Roles";

            return (await conn.QueryAsync<User>(sql, new { Roles = roles.Select(r => r.ToLower()) })).ToList();
        }


    }
}
