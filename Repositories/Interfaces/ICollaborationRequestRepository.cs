namespace KDomBackend.Repositories.Interfaces
{
    public interface ICollaborationRequestRepository
    {
        Task CreateAsync(KDomCollaborationRequest request);
        Task<bool> HasPendingAsync(string kdomId, int userId);
        Task<KDomCollaborationRequest?> GetByIdAsync(string requestId);
        Task UpdateAsync(KDomCollaborationRequest request);
        Task<List<KDomCollaborationRequest>> GetByKDomIdAsync(string kdomId);
        Task<List<KDomCollaborationRequest>> GetByUserIdAsync(int userId);

    }
}
