using KDomBackend.Models.MongoEntities;

namespace KDomBackend.Repositories.Interfaces
{
    public interface IKDomRepository
    {
        Task CreateAsync(KDom kdom);
        Task<KDom?> GetByIdAsync(string id);
        Task UpdateContentAsync(string id, string newContentHtml);
        Task SaveEditAsync(KDomEdit edit);
        Task UpdateMetadataAsync(KDomUpdateMetadataDto dto);
        Task SaveMetadataEditAsync(KDomMetadataEdit edit);
        Task<List<KDomEdit>> GetEditsByKDomIdAsync(string kdomId);
        Task<List<KDomMetadataEdit>> GetMetadataEditsByKDomIdAsync(string kdomId);

    }
}
