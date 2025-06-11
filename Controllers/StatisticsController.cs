// Controllers/StatisticsController.cs - FINAL VERSION cu error handling și logging
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/statistics")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(
            IStatisticsService statisticsService,
            ILogger<StatisticsController> logger)
        {
            _statisticsService = statisticsService;
            _logger = logger;
        }

        /// <summary>
        /// Obține statisticile generale ale platformei pentru homepage
        /// </summary>
        [HttpGet("platform")]
        public async Task<IActionResult> GetPlatformStats()
        {
            try
            {
                _logger.LogInformation("[StatisticsController] Getting platform stats...");

                var stats = await _statisticsService.GetPlatformStatsAsync();

                _logger.LogInformation("[StatisticsController] Platform stats retrieved successfully: {KDoms} K-Doms, {Users} users, {Collaborators} collaborators",
                    stats.TotalKDoms, stats.TotalUsers, stats.ActiveCollaborators);

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StatisticsController] Error getting platform stats");
                return BadRequest(new
                {
                    error = "Failed to retrieve platform statistics",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obține statisticile pe categorii pentru secțiunea "Explore K-Culture"
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategoryStats()
        {
            try
            {
                _logger.LogInformation("[StatisticsController] Getting category stats...");

                var stats = await _statisticsService.GetCategoryStatsAsync();

                _logger.LogInformation("[StatisticsController] Category stats retrieved: {Count} categories", stats.Count);

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StatisticsController] Error getting category stats");
                return BadRequest(new
                {
                    error = "Failed to retrieve category statistics",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obține K-Dom-urile featured pentru secțiunea "Popular K-Doms Right Now"
        /// </summary>
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedKDoms([FromQuery] int limit = 6)
        {
            try
            {
                _logger.LogInformation("[StatisticsController] Getting featured K-Doms with limit {Limit}...", limit);

                if (limit <= 0 || limit > 50)
                {
                    return BadRequest(new { error = "Limit must be between 1 and 50" });
                }

                var featured = await _statisticsService.GetFeaturedKDomsAsync(limit);

                _logger.LogInformation("[StatisticsController] Featured K-Doms retrieved: {Count} items", featured.Count);

                return Ok(featured);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StatisticsController] Error getting featured K-Doms");
                return BadRequest(new
                {
                    error = "Failed to retrieve featured K-Doms",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Obține toate datele necesare pentru homepage într-un singur request
        /// </summary>
        [HttpGet("homepage")]
        public async Task<IActionResult> GetHomepageData()
        {
            try
            {
                _logger.LogInformation("[StatisticsController] Getting complete homepage data...");

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var data = await _statisticsService.GetHomepageDataAsync();
                stopwatch.Stop();

                _logger.LogInformation("[StatisticsController] Homepage data retrieved successfully in {ElapsedMs}ms - Platform: {KDoms} K-Doms, Categories: {Categories}, Featured: {Featured}",
                    stopwatch.ElapsedMilliseconds,
                    data.PlatformStats.TotalKDoms,
                    data.CategoryStats.Count,
                    data.FeaturedKDoms.Count);

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StatisticsController] Error getting homepage data");
                return StatusCode(500, new
                {
                    error = "Failed to retrieve homepage data",
                    message = "Please try again later or contact support if the problem persists",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Health check endpoint pentru verificarea funcționalității
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                _logger.LogInformation("[StatisticsController] Health check requested...");

                // Test rapid pentru a verifica dacă serviciile funcționează
                var platformStats = await _statisticsService.GetPlatformStatsAsync();

                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    message = "Statistics service is operational",
                    platformStats = new
                    {
                        totalKDoms = platformStats.TotalKDoms,
                        totalUsers = platformStats.TotalUsers,
                        activeCollaborators = platformStats.ActiveCollaborators
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StatisticsController] Health check failed");
                return StatusCode(500, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Endpoint pentru debugging - returnează informații despre starea serviciului
        /// </summary>
        [HttpGet("debug")]
        public async Task<IActionResult> Debug()
        {
            try
            {
                _logger.LogInformation("[StatisticsController] Debug info requested...");

                var debug = new
                {
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    serviceStatus = "operational",
                    availableEndpoints = new[]
                    {
                        "GET /api/statistics/platform",
                        "GET /api/statistics/categories",
                        "GET /api/statistics/featured?limit=6",
                        "GET /api/statistics/homepage",
                        "GET /api/statistics/health"
                    }
                };

                return Ok(debug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StatisticsController] Debug endpoint failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}