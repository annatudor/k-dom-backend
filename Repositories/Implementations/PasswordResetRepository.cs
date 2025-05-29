using Dapper;
using System.Data;
using KDomBackend.Data;
using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;

namespace KDomBackend.Repositories.Implementations
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly DatabaseContext _context;

        public PasswordResetRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(PasswordResetToken token)
        {
            using var conn = _context.CreateConnection();

            const string sql = @"
                INSERT INTO password_reset_token (user_id, token, expires_at, used, created_at)
                VALUES (@UserId, @Token, @ExpiresAt, @Used, @CreatedAt)";

            await conn.ExecuteAsync(sql, token);
        }

        public async Task<PasswordResetToken?> GetByTokenAsync(string token)
        {
            using var conn = _context.CreateConnection();

            Console.WriteLine($"[DEBUG] Executing token query for: {token}");
            const string sql = @"
                            SELECT
                                id AS Id,
                                user_id AS UserId,
                                token AS Token,
                                expires_at AS ExpiresAt,
                                used AS Used,
                                created_at AS CreatedAt
                            FROM password_reset_token
                            WHERE BINARY token = @Token";

            Console.WriteLine($"[DEBUG] Querying token: {token}");

            var result = await conn.QueryFirstOrDefaultAsync<PasswordResetToken>(sql, new { Token = token.ToString() });

            Console.WriteLine(result != null
             ? $"[DEBUG] TOKEN FOUND! ID={result.Id}"
            : "[DEBUG] No token matched.");



            return result;  

        }

        public async Task MarkAsUsedAsync(int id)
        {
            using var conn = _context.CreateConnection();

            const string sql = "UPDATE password_reset_token SET used = TRUE WHERE id = @Id";
            await conn.ExecuteAsync(sql, new { Id = id });
            Console.WriteLine($"[DEBUG] Used flag updated for token ID: {id}");

        }
    }
}
