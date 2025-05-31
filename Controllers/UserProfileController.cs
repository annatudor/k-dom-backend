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
        /// </summary>
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var profile = await _userProfileService.GetUserProfileAsync(userId);
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
    }
}