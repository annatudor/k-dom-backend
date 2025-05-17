using KDomBackend.Models.MongoEntities;

namespace KDomBackend.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task CreateAsync(Notification notification);
        Task<List<Notification>> GetByUserIdAsync(int userId);
        Task<Notification?> GetByIdAsync(string id);
        Task MarkAsReadAsync(string id);
        Task<List<Notification>> GetUnreadByUserIdAsync(int userId);
        Task<int> CountUnreadAsync(int userId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteAsync(string id);


    }
}
