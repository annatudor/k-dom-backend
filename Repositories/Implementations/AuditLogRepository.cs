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

        public async Task<List<AuditLog>> GetModerationActionsAsync(int limit = 50)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"
        SELECT * FROM audit_log 
        WHERE action IN ('ApproveKDom', 'RejectKDom')
        ORDER BY created_at DESC 
        LIMIT @Limit";

            var result = await conn.QueryAsync<AuditLog>(sql, new { Limit = limit });
            return result.ToList();
        }

        public async Task<List<AuditLog>> GetModerationActionsByModeratorAsync(int moderatorId, DateTime? fromDate = null)
        {
            using var conn = _context.CreateConnection();
            var sql = @"
        SELECT * FROM audit_log 
        WHERE user_id = @ModeratorId 
        AND action IN ('ApproveKDom', 'RejectKDom')";

            if (fromDate.HasValue)
            {
                sql += " AND created_at >= @FromDate";
            }

            sql += " ORDER BY created_at DESC";

            var result = await conn.QueryAsync<AuditLog>(sql, new
            {
                ModeratorId = moderatorId,
                FromDate = fromDate
            });
            return result.ToList();
        }

        public async Task<List<AuditLog>> GetModerationActionsForKDomAsync(string kdomId)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"
        SELECT * FROM audit_log 
        WHERE target_id = @KDomId 
        AND action IN ('ApproveKDom', 'RejectKDom')
        ORDER BY created_at DESC";

            var result = await conn.QueryAsync<AuditLog>(sql, new { KDomId = kdomId });
            return result.ToList();
        }

        public async Task<Dictionary<int, int>> GetModeratorStatsAsync(DateTime fromDate)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"
        SELECT user_id, COUNT(*) as action_count
        FROM audit_log 
        WHERE action IN ('ApproveKDom', 'RejectKDom')
        AND created_at >= @FromDate
        AND user_id IS NOT NULL
        GROUP BY user_id";

            var result = await conn.QueryAsync<dynamic>(sql, new { FromDate = fromDate });
            return result.ToDictionary(
                r => (int)r.user_id,
                r => (int)r.action_count
            );
        }

        public async Task<AuditLog?> GetLastModerationActionAsync(string kdomId)
        {
            using var conn = _context.CreateConnection();
            const string sql = @"
        SELECT * FROM audit_log 
        WHERE target_id = @KDomId 
        AND action IN ('ApproveKDom', 'RejectKDom')
        ORDER BY created_at DESC 
        LIMIT 1";

            return await conn.QueryFirstOrDefaultAsync<AuditLog>(sql, new { KDomId = kdomId });
        }
    }
}
