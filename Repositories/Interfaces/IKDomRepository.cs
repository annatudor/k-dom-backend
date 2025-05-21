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
        Task<List<KDom>> GetPendingKdomsAsync();
        Task ApproveAsync(string kdomId);
        Task RejectAsync(string kdomId, string reason);
        Task<bool> ExistsByTitleOrSlugAsync(string title, string slug);
        Task<List<KDom>> FindSimilarByTitleAsync(string title);
        Task<List<KDom>> GetChildrenByParentIdAsync(string parentId);
        Task<KDom?> GetParentAsync(string childId);
        Task<List<KDom>> GetSiblingsAsync(string kdomId);
        Task UpdateCollaboratorsAsync(string kdomId, List<int> collaborators);

    }
}
