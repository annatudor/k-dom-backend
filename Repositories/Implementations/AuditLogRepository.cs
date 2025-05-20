using KDomBackend.Data;
using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;
using Dapper;

namespace KDomBackend.Repositories.Implementations
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly DatabaseContext _context;

        public AuditLogRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(AuditLog log)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"
            INSERT INTO audit_log (user_id, action, target_type, target_id, details, created_at)
            VALUES (@UserId, @Action, @TargetType, @TargetId, @Details, @CreatedAt)";
            await conn.ExecuteAsync(sql, log);
        }
        public async Task<List<AuditLog>> GetAllAsync()
        {
            using var conn = _context.CreateConnection();
            const string sql = "SELECT * FROM audit_log ORDER BY created_at DESC";
            var result = await conn.QueryAsync<AuditLog>(sql);
            return result.ToList();
        }

    }
}
