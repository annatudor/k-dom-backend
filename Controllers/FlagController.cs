using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KDomBackend.Models.DTOs.Flag;
using KDomBackend.Services.Interfaces;

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

        await _flagService.CreateFlagAsync(userId, dto);
        return Ok(new { message = "Thanks for your feedback! We use these reports to show you less of this content in the future." });
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

}
