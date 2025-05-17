using KDomBackend.Models.DTOs.Notification;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _notificationService.CreateNotificationAsync(dto);
            return Ok(new { message = "Notification created." });
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _notificationService.GetNotificationsAsync(userId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var notification = await _notificationService.GetByIdAsync(id, userId);
                return Ok(notification);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }


        [Authorize]
        [HttpPatch("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _notificationService.MarkAsReadAsync(id, userId);
                return Ok(new { message = "Marked as read." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _notificationService.GetUnreadNotificationsAsync(userId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { unreadCount = count });
        }

        [Authorize]
        [HttpPatch("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { message = "All notifications marked as read." });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _notificationService.DeleteNotificationAsync(id, userId);
                return Ok(new { message = "Notification deleted." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }


    }
}
