using System.Text.RegularExpressions;
using KDomBackend.Models.DTOs.Notification;
using KDomBackend.Enums;
using KDomBackend.Services.Interfaces;

public static class MentionHelper
{
    public static async Task HandleMentionsAsync(
    string text,
    int senderUserId,
    string targetId,
    ContentType targetType,
    NotificationType notificationType,
    IUserService userService,
    INotificationService notificationService)

    {
        var matches = Regex.Matches(text, @"@(\w+)");
        var mentionedUsernames = matches.Select(m => m.Groups[1].Value.ToLower()).Distinct();

        var senderUsername = await userService.GetUsernameByUserIdAsync(senderUserId);

        foreach (var username in mentionedUsernames)
        {
            var user = await userService.GetUserByUsernameAsync(username);
            if (user != null && user.Id != senderUserId)
            {
                await notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = user.Id,
                    Type = notificationType, // din parametru
                    Message = $"{senderUsername} mentioned you.",
                    TriggeredByUserId = senderUserId,
                    TargetType = targetType,
                    TargetId = targetId
                });

            }
        }
    }
}
