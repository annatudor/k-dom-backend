// Controllers/ViewTrackingController.cs - Controller actualizat pentru service-urile esențiale
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
                return Ok(new { 
                    message = "View tracked successfully.",
                    success = true 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    error = ex.Message,
                    success = false 
                });
            }
        }


        [HttpGet("count")]
        public async Task<IActionResult> GetViewCount([FromQuery] ContentType contentType, [FromQuery] string contentId)
        {
            if (string.IsNullOrWhiteSpace(contentId))
                return BadRequest(new { error = "Content ID is required." });

            try
            {
                var count = await _viewTrackingService.GetContentViewCountAsync(contentType, contentId);
                return Ok(new { 
                    contentType, 
                    contentId, 
                    viewCount = count 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("stats")]
        public async Task<IActionResult> GetViewStats([FromQuery] ContentType contentType, [FromQuery] string contentId)
        {
            if (string.IsNullOrWhiteSpace(contentId))
                return BadRequest(new { error = "Content ID is required." });

            try
            {
                var stats = await _viewTrackingService.GetDetailedStatsAsync(contentType, contentId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentViews(
            [FromQuery] ContentType contentType, 
            [FromQuery] string contentId,
            [FromQuery] int hours = 24)
        {
            if (string.IsNullOrWhiteSpace(contentId))
                return BadRequest(new { error = "Content ID is required." });

            try
            {
                var recentViews = await _viewTrackingService.GetRecentViewsAsync(contentType, contentId, hours);
                return Ok(new { 
                    contentType, 
                    contentId, 
                    hours,
                    recentViews 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

      
        [HttpGet("unique-viewers")]
        public async Task<IActionResult> GetUniqueViewers(
            [FromQuery] ContentType contentType, 
            [FromQuery] string contentId)
        {
            if (string.IsNullOrWhiteSpace(contentId))
                return BadRequest(new { error = "Content ID is required." });

            try
            {
                var uniqueViewers = await _viewTrackingService.GetUniqueViewersAsync(contentType, contentId);
                return Ok(new { 
                    contentType, 
                    contentId, 
                    uniqueViewers 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }




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

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingContent(
            [FromQuery] ContentType? contentType = null,
            [FromQuery] int hours = 24,
            [FromQuery] int limit = 10)
        {
            try
            {
                var trending = await _viewTrackingService.GetTrendingContentAsync(contentType, hours, limit);
                return Ok(new { 
                    contentType = contentType?.ToString() ?? "All",
                    hours,
                    trending 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

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

       

   
        [Authorize(Roles = "admin,moderator")]
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] int days = 30)
        {
            try
            {
                var analytics = await _viewTrackingService.GetAnalyticsAsync(days);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "admin,moderator")]
        [HttpGet("total-views")]
        public async Task<IActionResult> GetTotalViews([FromQuery] int days = 30)
        {
            try
            {
                var totalViews = await _viewTrackingService.GetTotalViewsAsync(days);
                return Ok(new { 
                    period = days,
                    totalViews 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [Authorize(Roles = "admin,moderator")]
        [HttpGet("content-type-breakdown")]
        public async Task<IActionResult> GetViewsByContentType([FromQuery] int days = 30)
        {
            try
            {
                var breakdown = await _viewTrackingService.GetViewsByContentTypeAsync(days);
                return Ok(new { 
                    period = days,
                    breakdown 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "admin,moderator")]
        [HttpGet("daily-views")]
        public async Task<IActionResult> GetDailyViews([FromQuery] int days = 30)
        {
            try
            {
                var dailyViews = await _viewTrackingService.GetDailyViewsAsync(days);
                return Ok(new { 
                    period = days,
                    dailyViews 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


    
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                // Test basic functionality
                var totalViews = await _viewTrackingService.GetTotalViewsAsync(1);
                
                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    message = "View tracking system is operational",
                    recentViews = totalViews
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                });
            }
        }

    }
}