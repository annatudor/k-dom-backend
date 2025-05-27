using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Notification;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Implementations;
using KDomBackend.Services.Interfaces;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;

    public FollowService(IFollowRepository repository, INotificationService notificationService, IUserRepository userRepository)
    {
        _repository = repository;
        _notificationService = notificationService;
        _userRepository = userRepository;
    }

    public async Task FollowUserAsync(int followerId, int followingId)
    {
        if (followerId == followingId)
            throw new Exception("You can't follow yourself.");

        var alreadyFollowing = await _repository.ExistsAsync(followerId, followingId);
        if (alreadyFollowing)
            throw new Exception("Already following.");

        await _repository.CreateAsync(followerId, followingId);

        await _notificationService.CreateNotificationAsync(new NotificationCreateDto
        {
            UserId = followingId,
            TriggeredByUserId = followerId,
            Type = NotificationType.NewFollower,
            Message = "You have a new follower!",
            TargetType = null,
            TargetId = null
        });


    }

    public async Task UnfollowUserAsync(int followerId, int followingId)
    {
        var alreadyFollowing = await _repository.ExistsAsync(followerId, followingId);
        if (!alreadyFollowing)
            throw new Exception("You're not following this user.");

        await _repository.DeleteAsync(followerId, followingId);
    }


    public async Task<List<UserPublicDto>> GetFollowersAsync(int userId)
    {
        var ids = await _repository.GetFollowersAsync(userId);
        var users = new List<UserPublicDto>();

        foreach (var id in ids)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null)
            {
                users.Add(new UserPublicDto
                {
                    Id = user.Id,
                    Username = user.Username
                    // avatar, bio etc.
                });
            }
        }

        return users;
    }

    public async Task<List<UserPublicDto>> GetFollowingAsync(int userId)
    {
        var ids = await _repository.GetFollowingAsync(userId);
        var users = new List<UserPublicDto>();

        foreach (var id in ids)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null)
            {
                users.Add(new UserPublicDto
                {
                    Id = user.Id,
                    Username = user.Username
                });
            }
        }

        return users;
    }

    public Task<int> GetFollowersCountAsync(int userId)
    => _repository.GetFollowersCountAsync(userId);

    public Task<int> GetFollowingCountAsync(int userId)
        => _repository.GetFollowingCountAsync(userId);


}
