using KDomBackend.Models.DTOs.Collaboration;

namespace KDomBackend.Services.Interfaces
{
    public interface ICollaborationStatsService
    {
        Task<KDomCollaborationStatsDto> GetKDomCollaborationStatsAsync(string kdomId, int requesterId);
        Task<List<CollaboratorReadDto>> GetCollaboratorsAsync(string kdomId, int requesterId);
    }
}
