﻿using KDomBackend.Models.MongoEntities;

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
        Task<List<KDom>> SearchTitleOrSlugByQueryAsync(string query);
        Task<List<KDom>> GetByIdsAsync(IEnumerable<string> ids);
        Task<List<KDom>> GetBySlugsAsync(IEnumerable<string> slugs);

        Task<Dictionary<string, int>> CountRecentEditsAsync(int days = 7);

        Task<List<KDom>> SearchByQueryAsync(string query);
        Task<List<KDom>> GetOwnedOrCollaboratedByUserAsync(int userId);
        Task<KDom?> GetBySlugAsync(string slug);

        Task<int> GetCreatedKDomsCountByUserAsync(int userId);
        Task<int> GetCollaboratedKDomsCountByUserAsync(int userId);
        Task<List<KDom>> GetKDomsByUserAsync(int userId, bool includeCollaborated = true);
        Task<int> GetUserKDomEditsCountAsync(int userId);
        Task<List<string>> GetUserKDomIdsAsync(int userId, bool includeCollaborated = true);
        Task<Dictionary<string, int>> GetUserKDomsByHubAsync(int userId);
        Task<Dictionary<string, int>> GetUserKDomsByLanguageAsync(int userId);

        Task<KDomEdit?> GetFirstEditByUserAsync(string kdomId, int userId);
        Task<List<KDomEdit>> GetEditsByUserAsync(string kdomId, int userId);
        Task<int> GetEditCountByUserAsync(string kdomId, int userId);

        Task DeleteAsync(string kdomId);
        Task<List<KDom>> GetKDomsByStatusAsync(bool isApproved, bool isRejected);
        Task<List<KDom>> GetUserKDomsWithStatusAsync(int userId);
        Task<int> GetPendingCountAsync();
        Task<List<KDom>> GetApprovedKdomsAsync();
        Task<List<KDom>> GetRejectedKdomsAsync();
        Task<int> GetApprovedCountAsync(DateTime? fromDate = null);
        Task<int> GetRejectedCountAsync(DateTime? fromDate = null);
        Task<List<KDom>> GetRecentlyModeratedAsync(int limit = 10);
        Task<TimeSpan> GetAverageProcessingTimeAsync();
        Task<DateTime?> GetModerationDateAsync(string kdomId);
        Task SetModeratorAsync(string kdomId, int moderatorUserId);
        Task<List<KDom>> GetApprovedKDomsAsync();
    }
}
