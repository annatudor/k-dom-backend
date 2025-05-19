using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KDomBackend.Services.Interfaces;

[ApiController]
[Route("api/follow")]
public class FollowController : ControllerBase
{
    private readonly IFollowService _followService;

    public FollowController(IFollowService followService)
    {
        _followService = followService;
    }

    [Authorize]
    [HttpPost("{userId}")]
    public async Task<IActionResult> Follow(int userId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _followService.FollowUserAsync(currentUserId, userId);
            return Ok(new { message = "User followed." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> Unfollow(int userId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _followService.UnfollowUserAsync(currentUserId, userId);
            return Ok(new { message = "User unfollowed." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("followers/{userId}")]
    public async Task<IActionResult> GetFollowers(int userId)
    {
        var result = await _followService.GetFollowersAsync(userId);
        return Ok(result);
    }

    [HttpGet("following/{userId}")]
    public async Task<IActionResult> GetFollowing(int userId)
    {
        var result = await _followService.GetFollowingAsync(userId);
        return Ok(result);
    }

    [HttpGet("followers-count/{userId}")]
    public async Task<IActionResult> GetFollowersCount(int userId)
    {
        var count = await _followService.GetFollowersCountAsync(userId);
        return Ok(new { followersCount = count });
    }

    [HttpGet("following-count/{userId}")]
    public async Task<IActionResult> GetFollowingCount(int userId)
    {
        var count = await _followService.GetFollowingCountAsync(userId);
        return Ok(new { followingCount = count });
    }


}
