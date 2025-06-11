using KDomBackend.Models.DTOs.Collaboration;
using KDomBackend.Models.DTOs.KDom;

namespace KDomBackend.Services.Interfaces
{
    public interface IKDomReadService
    {
        Task<KDomReadDto> GetKDomByIdAsync(string id);
        Task<List<KDomEditReadDto>> GetEditHistoryAsync(string kdomId, int userId);
        Task<List<KDomMetadataEditReadDto>> GetMetadataEditHistoryAsync(string kdomId, int userId);

        Task<KDomReadDto> GetKDomBySlugAsync(string slug);
        Task<List<KDomEditReadDto>> GetEditHistoryBySlugAsync(string slug, int userId);
        Task<List<KDomMetadataEditReadDto>> GetMetadataEditHistoryBySlugAsync(string slug, int userId);

        Task<List<KDomReadDto>> GetPendingKdomsAsync();

        Task<List<KDomReadDto>> GetChildrenAsync(string parentId);
        Task<KDomReadDto?> GetParentAsync(string childId);
        Task<List<KDomReadDto>> GetSiblingsAsync(string kdomId);

        Task<List<KDomTrendingDto>> GetTrendingKdomsAsync(int days = 7);
        Task<List<KDomTagSearchResultDto>> GetSuggestedKdomsAsync(int userId, int limit = 10);

        Task<List<KDomDisplayDto>> GetKdomsForUserAsync(int userId);

        Task<List<KDomDisplayDto>> GetRecentlyViewedKdomsAsync(int userId);
        Task<List<KDomTagSearchResultDto>> SearchTagOrSlugAsync(string query);
        Task<bool> ExistsByTitleOrSlugAsync(string title);
        Task<List<string>> GetSimilarTitlesAsync(string title);



    }
}
