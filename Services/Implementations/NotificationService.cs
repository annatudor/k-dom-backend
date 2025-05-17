using KDomBackend.Models.DTOs.Notification;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IUserService _userService;

        public NotificationService(INotificationRepository repository, IUserService userService)
        {
            _repository = repository;
            _userService = userService;
        }

        public async Task CreateNotificationAsync(NotificationCreateDto dto)
        {
            var notification = new Notification
            {
                UserId = dto.UserId,
                Type = dto.Type,
                Message = dto.Message,
                TriggeredByUserId = dto.TriggeredByUserId,
                TargetType = dto.TargetType,
                TargetId = dto.TargetId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _repository.CreateAsync(notification);
        }


        public async Task<List<NotificationReadDto>> GetNotificationsAsync(int userId)
        {
            var notifications = await _repository.GetByUserIdAsync(userId);

            var result = new List<NotificationReadDto>();

            foreach (var n in notifications)
            {
                var triggeredByUsername = n.TriggeredByUserId.HasValue
                    ? await _userService.GetUsernameByUserIdAsync(n.TriggeredByUserId.Value)
                    : null;

                result.Add(new NotificationReadDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    TriggeredByUsername = triggeredByUsername,
                    TargetType = n.TargetType,
                    TargetId = n.TargetId
                });
            }

            return result;
        }

        public async Task<NotificationReadDto> GetByIdAsync(string id, int userId)
        {
            var notification = await _repository.GetByIdAsync(id);
            if (notification == null)
                throw new Exception("Notification not found.");

            if (notification.UserId != userId)
                throw new UnauthorizedAccessException("Access denied.");

            var triggeredByUsername = notification.TriggeredByUserId.HasValue
                ? await _userService.GetUsernameByUserIdAsync(notification.TriggeredByUserId.Value)
                : null;

            return new NotificationReadDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                TriggeredByUsername = triggeredByUsername,
                TargetType = notification.TargetType,
                TargetId = notification.TargetId
            };
        }


        public async Task MarkAsReadAsync(string notificationId, int userId)
        {
            var notification = await _repository.GetByIdAsync(notificationId);
            if (notification == null)
                throw new Exception("Notification not found.");

            if (notification.UserId != userId)
                throw new UnauthorizedAccessException("This notification does not belong to you.");

            if (!notification.IsRead)
                await _repository.MarkAsReadAsync(notificationId);
        }

        public async Task<List<NotificationReadDto>> GetUnreadNotificationsAsync(int userId)
        {
            var notifications = await _repository.GetUnreadByUserIdAsync(userId);

            var result = new List<NotificationReadDto>();

            foreach (var n in notifications)
            {
                var triggeredByUsername = n.TriggeredByUserId.HasValue
                    ? await _userService.GetUsernameByUserIdAsync(n.TriggeredByUserId.Value)
                    : null;

                result.Add(new NotificationReadDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    TriggeredByUsername = triggeredByUsername,
                    TargetType = n.TargetType,
                    TargetId = n.TargetId
                });
            }

            return result;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _repository.CountUnreadAsync(userId);
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _repository.MarkAllAsReadAsync(userId);
        }

        public async Task DeleteNotificationAsync(string id, int userId)
        {
            var notification = await _repository.GetByIdAsync(id);
            if (notification == null)
                throw new Exception("Notification not found.");

            if (notification.UserId != userId)
                throw new UnauthorizedAccessException("You can't delete this notification.");

            await _repository.DeleteAsync(id);
        }


    }
}
