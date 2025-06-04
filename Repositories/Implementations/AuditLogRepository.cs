using KDomBackend.Data;
using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;
using Dapper;
using KDomBackend.Enums;

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
            
            var parameters = new
            {
                UserId = log.UserId,
                Action = log.Action.ToString(),      
                TargetType = log.TargetType.ToString(), 
                TargetId = log.TargetId,
                Details = log.Details,
                CreatedAt = log.CreatedAt
            };

            await conn.ExecuteAsync(sql, parameters);
           
        }
        public async Task<List<AuditLog>> GetAllAsync()
        {
            using var conn = _context.CreateConnection();
            const string sql = "SELECT * FROM audit_log ORDER BY created_at DESC";
            var result = await conn.QueryAsync<AuditLog>(sql);
            return result.ToList();
        }
        public async Task<AuditLog?> GetByKDomAndUserAsync(string kdomId, int userId, AuditAction action)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"
        SELECT * FROM audit_log 
        WHERE target_id = @KDomId 
        AND user_id = @UserId 
        AND action = @Action 
        ORDER BY created_at DESC 
        LIMIT 1";

            return await conn.QueryFirstOrDefaultAsync<AuditLog>(sql, new
            {
                KDomId = kdomId,
                UserId = userId,
                Action = action.ToString()
            });
        }
    }
}
