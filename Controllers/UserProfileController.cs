// Controllers/UserProfileController.cs - FIXED VERSION
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KDomBackend.Services.Interfaces;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Enums;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize] // Toate endpoint-urile necesită autentificare
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IKDomReadService _kdomReadService;

        public UserProfileController(
            IUserProfileService userProfileService,
            IKDomReadService kdomReadService)
        {
            _userProfileService = userProfileService;
            _kdomReadService = kdomReadService;
        }

        /// <summary>
        /// Obține profilul utilizatorului curent (din JWT token) - FIXED VERSION
        /// </summary>
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var profile = await _userProfileService.GetUserProfileAsync(userId, userId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetMyProfile: {ex.Message}");
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Actualizează profilul utilizatorului curent - FIXED VERSION
        /// </summary>
        [HttpPut("edit-profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UserProfileUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _userProfileService.UpdateProfileAsync(userId, dto);
                return Ok(new { message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdateMyProfile: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține temele disponibile pentru profil
        /// </summary>
        [HttpGet("themes")]
        public IActionResult GetAvailableThemes()
        {
            var themes = Enum.GetNames(typeof(ProfileTheme));
            return Ok(themes);
        }

        /// <summary>
        /// Obține K-Dom-urile pe care utilizatorul le-a vizionat recent
        /// </summary>
        [HttpGet("recently-viewed-kdoms")]
        public async Task<IActionResult> GetRecentlyViewedKdoms()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                Console.WriteLine($"[DEBUG] GetRecentlyViewedKdoms called for user {userId}");

                // Obținem ID-urile
                var ids = await _userProfileService.GetRecentlyViewedKDomIdsAsync(userId);
                Console.WriteLine($"[DEBUG] Found {ids.Count} recently viewed IDs: [{string.Join(", ", ids)}]");

                if (!ids.Any())
                {
                    Console.WriteLine($"[DEBUG] No recently viewed KDoms for user {userId}");
                    return Ok(new List<object>());
                }

                // Obținem K-DOM-urile complete
                var result = await _kdomReadService.GetRecentlyViewedKdomsAsync(userId);
                Console.WriteLine($"[DEBUG] Converted to {result.Count} KDom objects");

                foreach (var kdom in result)
                {
                    Console.WriteLine($"[DEBUG] KDom: {kdom.Id} - {kdom.Title}");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetRecentlyViewedKdoms: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține K-Dom-urile create sau colaborate de utilizatorul curent
        /// </summary>
        [HttpGet("my-kdoms")]
        public async Task<IActionResult> GetMyKdoms()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _kdomReadService.GetKdomsForUserAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetMyKdoms: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține informațiile private ale utilizatorului curent - FIXED VERSION
        /// </summary>
        [HttpGet("private")]
        public async Task<IActionResult> GetMyPrivateInfo()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var privateInfo = await _userProfileService.GetUserPrivateInfoAsync(userId, userId);
                return Ok(privateInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetMyPrivateInfo: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Adaugă un K-Dom la lista de recent vizualizate
        /// </summary>
        [HttpPost("recently-viewed/{kdomId}")]
        public async Task<IActionResult> AddRecentlyViewedKdom(string kdomId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                Console.WriteLine($"[DEBUG] AddRecentlyViewedKdom called: user {userId}, kdom {kdomId}");

                // Validăm că K-DOM-ul există
                var kdom = await _kdomReadService.GetKDomByIdAsync(kdomId);
                if (kdom == null)
                {
                    Console.WriteLine($"[ERROR] KDom {kdomId} not found");
                    return NotFound(new { error = "K-Dom not found." });
                }

                Console.WriteLine($"[DEBUG] K-Dom found: {kdom.Title}");

                await _userProfileService.AddRecentlyViewedKDomAsync(userId, kdomId);
                Console.WriteLine($"[DEBUG] Successfully added K-Dom {kdomId} to recently viewed");

                return Ok(new { message = "K-Dom added to recently viewed." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AddRecentlyViewedKdom: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verifică dacă utilizatorul curent poate actualiza un profil specific
        /// </summary>
        [HttpGet("can-update/{targetUserId}")]
        public async Task<IActionResult> CanUpdateProfile(int targetUserId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var canUpdate = await _userProfileService.CanUserUpdateProfileAsync(currentUserId, targetUserId);
                return Ok(new { canUpdate });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CanUpdateProfile: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține statisticile detaliate ale utilizatorului curent - REMOVED ADMIN RESTRICTION
        /// </summary>
        [HttpGet("detailed-stats")]
        public async Task<IActionResult> GetMyDetailedStats()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // Check if user is admin or viewing their own stats
                var isAdmin = await _userProfileService.IsUserAdminAsync(userId);
                if (!isAdmin)
                {
                    // For non-admin users, return basic stats only
                    return Ok(new { message = "Detailed stats available for administrators only" });
                }

                var stats = await _userProfileService.GetUserDetailedStatsAsync(userId, userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetMyDetailedStats: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verifică dacă utilizatorul curent este admin
        /// </summary>
        [HttpGet("is-admin")]
        public async Task<IActionResult> IsCurrentUserAdmin()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var isAdmin = await _userProfileService.IsUserAdminAsync(userId);
                return Ok(new { isAdmin });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] IsCurrentUserAdmin: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verifică dacă utilizatorul curent este admin sau moderator
        /// </summary>
        [HttpGet("is-admin-or-moderator")]
        public async Task<IActionResult> IsCurrentUserAdminOrModerator()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var isAdminOrMod = await _userProfileService.IsUserAdminOrModeratorAsync(userId);
                return Ok(new { isAdminOrModerator = isAdminOrMod });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] IsCurrentUserAdminOrModerator: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint pentru a obține lista ID-urilor K-Dom-urilor recent vizualizate
        /// </summary>
        [HttpGet("recently-viewed-ids")]
        public async Task<IActionResult> GetRecentlyViewedKDomIds()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                Console.WriteLine($"[DEBUG] GetRecentlyViewedKDomIds called for user {userId}");

                var ids = await _userProfileService.GetRecentlyViewedKDomIdsAsync(userId);
                Console.WriteLine($"[DEBUG] Found {ids.Count} recently viewed IDs");

                return Ok(new { kdomIds = ids });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetRecentlyViewedKDomIds: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint de test pentru a valida permisiunile de actualizare
        /// </summary>
        [HttpPost("validate-update-permissions/{targetUserId}")]
        public async Task<IActionResult> ValidateUpdatePermissions(int targetUserId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _userProfileService.ValidateUpdatePermissionsAsync(currentUserId, targetUserId);
                return Ok(new { message = "Permissions validated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ValidateUpdatePermissions: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}