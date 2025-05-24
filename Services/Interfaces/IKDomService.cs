using KDomBackend.Models.DTOs.Collaboration;
using KDomBackend.Models.DTOs.KDom;

namespace KDomBackend.Services.Interfaces
{
    public interface IKDomService
    {
        Task CreateKDomAsync(KDomCreateDto dto, int userId);
        Task<bool> EditKDomAsync(KDomEditDto dto, int userId);
        Task<bool> UpdateKDomMetadataAsync(KDomUpdateMetadataDto dto, int userId);
        Task<KDomReadDto> GetKDomByIdAsync(string id);
        Task<List<KDomEditReadDto>> GetEditHistoryAsync(string kdomId, int userId);
        Task<List<KDomMetadataEditReadDto>> GetMetadataEditHistoryAsync(string kdomId, int userId);
        Task<List<KDomReadDto>> GetPendingKdomsAsync();
        Task ApproveKdomAsync(string kdomId, int moderatorId);
        Task RejectKdomAsync(string kdomId, KDomRejectDto dto, int moderatorId);
        Task<List<KDomReadDto>> GetChildrenAsync(string parentId);
        Task<KDomReadDto?> GetParentAsync(string childId);
        Task<List<KDomReadDto>> GetSiblingsAsync(string kdomId);
        Task<List<CollaboratorReadDto>> GetCollaboratorsAsync(string kdomId, int requesterId);
        Task RemoveCollaboratorAsync(string kdomId, int requesterId, int userIdToRemove);
        Task CreateSubKDomAsync(string parentId, KDomSubCreateDto dto, int userId);
        Task<List<KDomTagSearchResultDto>> SearchTagOrSlugAsync(string query);
        Task<List<KDomTrendingDto>> GetTrendingKdomsAsync(int days = 7);
        Task<List<KDomTagSearchResultDto>> GetSuggestedKdomsAsync(int userId, int limit = 10);
        Task<List<KDomDisplayDto>> GetKdomsForUserAsync(int userId);
        Task<List<KDomDisplayDto>> GetRecentlyViewedKdomsAsync(int userId);
    }
}
