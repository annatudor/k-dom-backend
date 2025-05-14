using KDomBackend.Models.DTOs.KDom;

namespace KDomBackend.Services.Interfaces
{
    public interface IKDomService
    {
        Task CreateKDomAsync(KDomCreateDto dto, int userId);
        Task<bool> EditKDomAsync(KDomEditDto dto, int userId, bool isAutoSave = true);
        Task<bool> UpdateKDomMetadataAsync(KDomUpdateMetadataDto dto, int userId);
        Task<KDomReadDto> GetKDomByIdAsync(string id);
        Task<List<KDomEditReadDto>> GetEditHistoryAsync(string kdomId, int userId);
        Task<List<KDomMetadataEditReadDto>> GetMetadataEditHistoryAsync(string kdomId, int userId);


    }
}
