// Controllers/ViewTrackingController.cs
using KDomBackend.Enums;
using KDomBackend.Models.DTOs.ViewTracking;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/view-tracking")]
    public class ViewTrackingController : ControllerBase
    {
        private readonly IViewTrackingService _viewTrackingService;

        public ViewTrackingController(IViewTrackingService viewTrackingService)
        {
            _viewTrackingService = viewTrackingService;
        }

        /// <summary>
        /// Înregistrează o vizualizare de conținut
        /// </summary>
        [HttpPost("track")]
        public async Task<IActionResult> TrackView([FromBody] ViewTrackingCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Încearcă să obțină user ID din token dacă utilizatorul este logat
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (int.TryParse(userIdClaim, out var userId))
                    {
                        dto.ViewerId = userId;
                    }
                }

                // Obține IP-ul din request
                dto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                dto.UserAgent = Request.Headers["User-Agent"].ToString();

                await _viewTrackingService.TrackViewAsync(dto);
                return Ok(new { message = "View tracked successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține numărul de vizualizări pentru un conținut specific
        /// </summary>
        [HttpGet("count")]
        public async Task<IActionResult> GetViewCount([FromQuery] ContentType contentType, [FromQuery] string contentId)
        {
            if (string.IsNullOrWhiteSpace(contentId))
                return BadRequest(new { error = "Content ID is required." });

            try
            {
                var count = await _viewTrackingService.GetContentViewCountAsync(contentType, contentId);
                return Ok(new { contentType, contentId, viewCount = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține top conținut vizualizat (pentru o anumită categorie)
        /// </summary>
        [HttpGet("top-viewed")]
        public async Task<IActionResult> GetTopViewed([FromQuery] ContentType contentType, [FromQuery] int limit = 10)
        {
            try
            {
                var topViewed = await _viewTrackingService.GetTopViewedContentAsync(contentType, limit);
                return Ok(new { contentType, topViewed });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține statistici de vizualizări pentru utilizatorul curent (admin only)
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpGet("user-stats/{userId}")]
        public async Task<IActionResult> GetUserViewStats(int userId)
        {
            try
            {
                var totalViews = await _viewTrackingService.GetUserTotalViewsAsync(userId);
                var breakdown = await _viewTrackingService.GetUserViewsBreakdownAsync(userId);

                return Ok(new
                {
                    userId,
                    totalViews,
                    breakdown
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține statistici de vizualizări pentru utilizatorul curent
        /// </summary>
        [Authorize]
        [HttpGet("my-stats")]
        public async Task<IActionResult> GetMyViewStats()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var totalViews = await _viewTrackingService.GetUserTotalViewsAsync(userId);
                var breakdown = await _viewTrackingService.GetUserViewsBreakdownAsync(userId);

                return Ok(new
                {
                    totalViews,
                    breakdown,
                    message = "Your content view statistics"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}