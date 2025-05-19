using System.Data;
using Dapper;
using KDomBackend.Data;
using KDomBackend.Repositories.Interfaces;

public class FollowRepository : IFollowRepository
{
    private readonly DatabaseContext _context;

    public FollowRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(int followerId, int followingId)
    {
        using var conn = _context.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM follows WHERE follower_id = @followerId AND following_id = @followingId";
        return await conn.ExecuteScalarAsync<bool>(sql, new { followerId, followingId });
    }

    public async Task CreateAsync(int followerId, int followingId)
    {
        using var conn = _context.CreateConnection();
        const string sql = "INSERT INTO follows (follower_id, following_id) VALUES (@followerId, @followingId)";
        await conn.ExecuteAsync(sql, new { followerId, followingId });
    }

    public async Task DeleteAsync(int followerId, int followingId)
    {
        using var conn = _context.CreateConnection();
        const string sql = "DELETE FROM follows WHERE follower_id = @followerId AND following_id = @followingId";
        await conn.ExecuteAsync(sql, new { followerId, followingId });
    }

    public async Task<List<int>> GetFollowersAsync(int userId)
    {
        using var conn = _context.CreateConnection();
        const string sql = "SELECT follower_id FROM follows WHERE following_id = @userId";
        var result = await conn.QueryAsync<int>(sql, new { userId });
        return result.ToList();
    }

    public async Task<List<int>> GetFollowingAsync(int userId)
    {
        using var conn = _context.CreateConnection();
        const string sql = "SELECT following_id FROM follows WHERE follower_id = @userId";
        var result = await conn.QueryAsync<int>(sql, new { userId });
        return result.ToList();
    }

    public async Task<int> GetFollowersCountAsync(int userId)
    {
        using var conn = _context.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM follows WHERE following_id = @userId";
        return await conn.ExecuteScalarAsync<int>(sql, new { userId });
    }

    public async Task<int> GetFollowingCountAsync(int userId)
    {
        using var conn = _context.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM follows WHERE follower_id = @userId";
        return await conn.ExecuteScalarAsync<int>(sql, new { userId });
    }


}
