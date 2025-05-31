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
            const string sql = @"SELECT 
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
                WHERE id = @Id";
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

        public async Task<List<User>> SearchUsersAsync(string query)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"SELECT id, username FROM users WHERE username LIKE CONCAT('%', @Query, '%') LIMIT 10";

            return (await conn.QueryAsync<User>(sql, new { Query = query })).ToList();
        }

        public async Task<int> GetUserKDomsCreatedCountAsync(int userId)
        {
            using var conn = _context.CreateConnection();
            // Presupunem că avem o coloană user_id în tabela kdoms sau folosim MongoDB
            // Pentru MongoDB, va trebui să apelăm IKDomRepository
            // Pentru acum, returnăm 0 și implementăm în service prin IKDomRepository
            return 0; // Implementare în service
        }

        public async Task<int> GetUserKDomsCollaboratedCountAsync(int userId)
        {
            // Similar, implementare în service prin IKDomRepository
            return 0;
        }

        public async Task<int> GetUserPostsCountAsync(int userId)
        {
            // Similar, implementare în service prin IPostRepository
            return 0;
        }

        public async Task<int> GetUserCommentsCountAsync(int userId)
        {
            // Similar, implementare în service prin ICommentRepository
            return 0;
        }

        public async Task<DateTime?> GetUserLastActivityAsync(int userId)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"
                SELECT MAX(created_at) 
                FROM audit_log 
                WHERE user_id = @UserId 
                AND action IN ('CreateKDom', 'EditKDom', 'CreatePost', 'CreateComment')";

            return await conn.QueryFirstOrDefaultAsync<DateTime?>(sql, new { UserId = userId });
        }

        public async Task<int> GetUserTotalLikesReceivedAsync(int userId)
        {
            // Implementare prin aggregare din MongoDB (posts + comments)
            return 0; // Implementare în service
        }

        public async Task<int> GetUserTotalLikesGivenAsync(int userId)
        {
            // Implementare prin aggregare din MongoDB
            return 0; // Implementare în service
        }

        public async Task<int> GetUserCommentsReceivedAsync(int userId)
        {
            // Implementare prin ICommentRepository
            return 0; // Implementare în service
        }

        public async Task<int> GetUserFlagsReceivedAsync(int userId)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) 
                FROM flags f
                JOIN posts p ON f.content_id = p.id AND f.content_type = 'Post'
                WHERE p.user_id = @UserId
                UNION ALL
                SELECT COUNT(*) 
                FROM flags f
                JOIN comments c ON f.content_id = c.id AND f.content_type = 'Comment'
                WHERE c.user_id = @UserId";

            var results = await conn.QueryAsync<int>(sql, new { UserId = userId });
            return results.Sum();
        }

        public async Task<Dictionary<string, int>> GetUserActivityByMonthAsync(int userId, int months = 12)
        {
            using var conn = _context.CreateConnection();
            var fromDate = DateTime.UtcNow.AddMonths(-months);

            const string sql = @"
                SELECT 
                    DATE_FORMAT(created_at, '%Y-%m') as month,
                    COUNT(*) as count
                FROM audit_log 
                WHERE user_id = @UserId 
                AND created_at >= @FromDate
                AND action IN ('CreateKDom', 'EditKDom', 'CreatePost', 'CreateComment')
                GROUP BY DATE_FORMAT(created_at, '%Y-%m')
                ORDER BY month";

            var results = await conn.QueryAsync<dynamic>(sql, new { UserId = userId, FromDate = fromDate });

            return results.ToDictionary(
                r => (string)r.month,
                r => (int)r.count
            );
        }

        public async Task<List<string>> GetUserRecentActionsAsync(int userId, int limit = 10)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"
                SELECT CONCAT(action, ' - ', details, ' (', created_at, ')') as action_description
                FROM audit_log 
                WHERE user_id = @UserId 
                ORDER BY created_at DESC 
                LIMIT @Limit";

            var results = await conn.QueryAsync<string>(sql, new { UserId = userId, Limit = limit });
            return results.ToList();
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            using var conn = _context.CreateConnection();
            const string sql = "UPDATE users SET last_login_at = @LoginTime WHERE id = @UserId";
            await conn.ExecuteAsync(sql, new { LoginTime = DateTime.UtcNow, UserId = userId });
        }

        public async Task<DateTime?> GetLastLoginAsync(int userId)
        {
            using var conn = _context.CreateConnection();
            const string sql = "SELECT last_login_at FROM users WHERE id = @UserId";
            return await conn.QueryFirstOrDefaultAsync<DateTime?>(sql, new { UserId = userId });
        }

    }
}
