using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KDomBackend.Models.DTOs.Flag;
using KDomBackend.Services.Interfaces;
using KDomBackend.Enums;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/flags")]
public class FlagController : ControllerBase
{
    private readonly IFlagService _flagService;

    public FlagController(IFlagService flagService)
    {
        _flagService = flagService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FlagCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _flagService.CreateFlagAsync(userId, dto);
            var message = GetSuccessMessage(dto.ContentType);
            return Ok(new { message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ✅ ENHANCED: Acum returnează conținutul flagged pentru review
    [Authorize(Roles = "admin,moderator")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var flags = await _flagService.GetAllAsync();
            var pendingFlags = flags.Where(f => !f.IsResolved).ToList();
            var resolvedFlags = flags.Where(f => f.IsResolved).ToList();

            return Ok(new
            {
                pending = pendingFlags,
                resolved = resolvedFlags,
                summary = new
                {
                    totalPending = pendingFlags.Count,
                    totalResolved = resolvedFlags.Count,
                    total = flags.Count
                },
                message = pendingFlags.Any()
                    ? $"You have {pendingFlags.Count} pending flag(s) to review"
                    : "No pending flags to review"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ✅ NEW: Endpoint pentru a rezolva flag-ul (conținutul e ok)
    [Authorize(Roles = "admin,moderator")]
    [HttpPost("{id}/resolve")]
    public async Task<IActionResult> Resolve(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _flagService.ResolveAsync(id, userId);
            return Ok(new
            {
                message = "Flag resolved. Content remains available.",
                action = "resolved"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ✅ NEW: Endpoint pentru a șterge conținutul flagged
    [Authorize(Roles = "admin,moderator")]
    [HttpPost("{id}/remove-content")]
    public async Task<IActionResult> RemoveContent(int id, [FromBody] ContentRemovalDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _flagService.DeleteFlaggedContentAsync(id, userId, dto.Reason);

            return Ok(new
            {
                message = "Flagged content has been removed and author notified.",
                action = "content_removed"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ✅ KEPT: Admin poate șterge flag-ul în sine
    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _flagService.DeleteAsync(id, userId);
            return Ok(new { message = "Flag deleted." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ✅ NEW: Statistici pentru dashboard
    [Authorize(Roles = "admin,moderator")]
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var allFlags = await _flagService.GetAllAsync();
            var pendingCount = allFlags.Count(f => !f.IsResolved);
            var todayFlags = allFlags.Count(f => f.CreatedAt.Date == DateTime.Today);

            var flagsByType = allFlags.GroupBy(f => f.ContentType.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            return Ok(new
            {
                totalPending = pendingCount,
                totalToday = todayFlags,
                totalAllTime = allFlags.Count,
                flagsByContentType = flagsByType,
                requiresAttention = pendingCount > 0
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static string GetSuccessMessage(ContentType contentType)
    {
        return contentType switch
        {
            ContentType.KDom => "Thanks for your feedback! K-Dom reports help us maintain quality content and appropriate categorization. We'll review this report and take appropriate action.",
            ContentType.Post => "Thanks for your feedback! Post reports help us keep discussions relevant and respectful. We'll review this report promptly.",
            ContentType.Comment => "Thanks for your feedback! Comment reports help us maintain constructive conversations. We'll review this report and take appropriate action.",
            _ => "Thanks for your feedback! We use these reports to show you less of this content in the future."
        };
    }
}


