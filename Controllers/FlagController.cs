using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KDomBackend.Models.DTOs.Flag;
using KDomBackend.Services.Interfaces;
using KDomBackend.Enums;

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

            // Return content-specific success messages
            var message = GetSuccessMessage(dto.ContentType);

            return Ok(new { message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Roles = "admin,moderator")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var flags = await _flagService.GetAllAsync();
        return Ok(flags);
    }

    [Authorize(Roles = "admin,moderator")]
    [HttpPost("{id}/resolve")]
    public async Task<IActionResult> Resolve(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _flagService.ResolveAsync(id, userId);
        return Ok(new { message = "Report marked as resolved." });
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _flagService.DeleteAsync(id, userId);

        return Ok(new { message = "Report deleted." });
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