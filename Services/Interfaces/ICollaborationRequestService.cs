using KDomBackend.Models.DTOs.Collaboration;

namespace KDomBackend.Services.Interfaces
{
    public interface ICollaborationRequestService
    {
        Task CreateRequestAsync(string kdomId, int userId, CollaborationRequestCreateDto dto);
        Task ApproveAsync(string kdomId, string requestId, int reviewerId);
        Task RejectAsync(string kdomId, string requestId, int reviewerId, string? reason);
        Task<List<CollaborationRequestReadDto>> GetRequestsAsync(string kdomId, int userId);


    }
}
