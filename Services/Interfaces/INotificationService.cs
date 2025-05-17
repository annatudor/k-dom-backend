using KDomBackend.Models.DTOs.Notification;

namespace KDomBackend.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(NotificationCreateDto dto);
        Task<List<NotificationReadDto>> GetNotificationsAsync(int userId);
        Task<NotificationReadDto> GetByIdAsync(string id, int userId);
        Task MarkAsReadAsync(string notificationId, int userId);
        Task<List<NotificationReadDto>> GetUnreadNotificationsAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteNotificationAsync(string id, int userId);

    }
}
