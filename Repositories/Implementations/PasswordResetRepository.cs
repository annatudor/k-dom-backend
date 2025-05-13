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
                INSERT INTO password_resets (user_id, token, expires_at, used, created_at)
                VALUES (@UserId, @Token, @ExpiresAt, @Used, @CreatedAt)";

            await conn.ExecuteAsync(sql, token);
        }

        public async Task<PasswordResetToken?> GetByTokenAsync(string token)
        {
            using var conn = _context.CreateConnection();

            const string sql = @"
                SELECT 
                    id, user_id, token, expires_at, used, created_at
                FROM password_resets
                WHERE token = @Token";

            return await conn.QueryFirstOrDefaultAsync<PasswordResetToken>(sql, new { Token = token });
        }

        public async Task MarkAsUsedAsync(int id)
        {
            using var conn = _context.CreateConnection();

            const string sql = "UPDATE password_resets SET used = TRUE WHERE id = @Id";
            await conn.ExecuteAsync(sql, new { Id = id });
        }
    }
}
