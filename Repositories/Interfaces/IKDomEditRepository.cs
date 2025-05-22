namespace KDomBackend.Repositories.Interfaces
{
    public interface IKDomEditRepository
    {
        Task<List<string>> GetEditedKDomIdsByUserAsync(int userId, int days = 30);
        Task<Dictionary<string, int>> CountRecentEditsAsync(int days = 7); // pentru trending
    }
}
