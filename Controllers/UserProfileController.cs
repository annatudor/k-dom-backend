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
        /// Obține profilul utilizatorului curent (din JWT token)
        /// Folosește GetEnhancedUserProfileAsync cu viewerId pentru a include date private
        /// </summary>
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                // Folosește metoda enhanced cu viewerId pentru a include toate datele private
                var profile = await _userProfileService.GetUserProfileAsync(userId, userId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Actualizează profilul utilizatorului curent
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
                var result = await _kdomReadService.GetRecentlyViewedKdomsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
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
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține informațiile private ale utilizatorului curent
        /// (email, rol, provider, etc.)
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
                await _userProfileService.AddRecentlyViewedKDomAsync(userId, kdomId);
                return Ok(new { message = "K-Dom added to recently viewed." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verifică dacă utilizatorul curent poate actualiza un profil specific
        /// Util pentru frontend-ul de admin
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
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține statisticile detaliate ale utilizatorului curent (admin only)
        /// </summary>
        [HttpGet("detailed-stats")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetMyDetailedStats()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var stats = await _userProfileService.GetUserDetailedStatsAsync(userId, userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verifică dacă utilizatorul curent este admin
        /// Util pentru frontend pentru a arăta/ascunde funcționalități
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
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint pentru a obține lista ID-urilor K-Dom-urilor recent vizualizate
        /// Util pentru sincronizare cu frontend storage
        /// </summary>
        [HttpGet("recently-viewed-ids")]
        public async Task<IActionResult> GetRecentlyViewedKDomIds()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var ids = await _userProfileService.GetRecentlyViewedKDomIdsAsync(userId);
                return Ok(new { kdomIds = ids });
            }
            catch (Exception ex)
            {
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
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}