using KDomBackend.Models.DTOs.KDom;

namespace KDomBackend.Services.Interfaces
{
    public interface IKDomFlowService
    {
        Task CreateKDomAsync(KDomCreateDto dto, int userId);
        Task CreateSubKDomAsync(string parentId, KDomSubCreateDto dto, int userId);
        
        Task<bool> EditKDomAsync(KDomEditDto dto, int userId);
        Task<bool> EditKDomBySlugAsync(KDomEditDto dto, int userId);

        Task RemoveCollaboratorAsync(string kdomId, int requesterId, int userIdToRemove);


        Task<bool> UpdateKDomMetadataByIdAsync(string kdomId, KDomUpdateMetadataDto dto, int userId);
        Task<bool> UpdateKDomMetadataBySlugAsync(string slug, KDomUpdateMetadataDto dto, int userId);
    }
}
