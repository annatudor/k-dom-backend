using Dapper;
using System.Data;
using KDomBackend.Data;
using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;

public class FlagRepository : IFlagRepository
{
    private readonly DatabaseContext _context;

    public FlagRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Flag flag)
    {
        using var conn = _context.CreateConnection();
        const string sql = @"
            INSERT INTO flags (user_id, content_type, content_id, reason, created_at)
            VALUES (@UserId, @ContentType, @ContentId, @Reason, @CreatedAt)";
        await conn.ExecuteAsync(sql, flag);
    }

    public async Task<List<Flag>> GetAllAsync()
    {
        using var conn = _context.CreateConnection();
        const string sql = "SELECT * FROM flags ORDER BY created_at DESC";
        var result = await conn.QueryAsync<Flag>(sql);
        return result.ToList();
    }

    public async Task MarkResolvedAsync(int id)
    {
        using var conn = _context.CreateConnection();
        const string sql = "UPDATE flags SET is_resolved = TRUE WHERE id = @id";
        await conn.ExecuteAsync(sql, new { id });
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _context.CreateConnection();
        const string sql = "DELETE FROM flags WHERE id = @id";
        await conn.ExecuteAsync(sql, new { id });
    }
    public async Task<Flag?> GetFlagByIdAsync(int id)
    {
        using var conn = _context.CreateConnection();
        const string sql = "SELECT * FROM flags WHERE id = @id";
        return await conn.QueryFirstOrDefaultAsync<Flag>(sql, new { id });
    }

}
