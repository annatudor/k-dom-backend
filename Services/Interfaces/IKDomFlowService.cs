using KDomBackend.Models.DTOs.KDom;

namespace KDomBackend.Services.Interfaces
{
    public interface IKDomFlowService
    {
        Task CreateKDomAsync(KDomCreateDto dto, int userId);
        Task CreateSubKDomAsync(string parentId, KDomSubCreateDto dto, int userId);
        Task<bool> EditKDomAsync(KDomEditDto dto, int userId);
        Task<bool> UpdateKDomMetadataAsync(KDomUpdateMetadataDto dto, int userId);
        Task ApproveKdomAsync(string kdomId, int moderatorId);
        Task RejectKdomAsync(string kdomId, KDomRejectDto dto, int moderatorId);
        Task RemoveCollaboratorAsync(string kdomId, int requesterId, int userIdToRemove);
    }
}
