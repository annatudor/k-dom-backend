using KDomBackend.Models.DTOs.Moderation;

namespace KDomBackend.Services.Interfaces
{
    public interface IModerationService
    {
        // Dashboard pentru moderatori
        Task<ModerationDashboardDto> GetModerationDashboardAsync(int moderatorId);

        // Acțiuni de moderare
        Task<BulkModerationResultDto> BulkModerateAsync(BulkModerationDto dto, int moderatorId);
        Task RejectAndDeleteKDomAsync(string kdomId, string reason, int moderatorId);

        // Pentru utilizatori
        Task<ModerationHistoryDto> GetUserModerationHistoryAsync(int userId);
        Task<List<UserKDomStatusDto>> GetUserKDomStatusesAsync(int userId);
        Task<UserKDomStatusDto> GetKDomStatusAsync(string kdomId, int userId);

        // Statistici și rapoarte
        Task<ModerationStatsDto> GetModerationStatsAsync();
        Task<List<ModerationActionDto>> GetRecentModerationActionsAsync(int limit = 20);
        Task<List<ModeratorActivityDto>> GetTopModeratorsAsync(int days = 30, int limit = 10);

        // Utilități
        Task<bool> CanUserViewKDomStatusAsync(string kdomId, int userId);
        Task<ModerationPriority> CalculateKDomPriorityAsync(string kdomId);

        Task ApproveKDomAsync(string kdomId, int moderatorId);
        Task RejectKDomAsync(string kdomId, string reason, int moderatorId);
        Task ForceDeleteKDomAsync(string kdomId, int requesterId, string reason);
    }
}