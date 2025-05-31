using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KDomBackend.Services.Interfaces;
using KDomBackend.Models.DTOs.User;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "admin")] // Doar admini pot accesa aceste endpoint-uri
    public class UserAdminController : ControllerBase
    {
        private readonly IUserAdminService _userAdminService;
        private readonly IUserProfileService _userProfileService;
        private readonly IKDomReadService _kdomReadService;

        public UserAdminController(
            IUserAdminService userAdminService,
            IUserProfileService userProfileService,
            IKDomReadService kdomReadService)
        {
            _userAdminService = userAdminService;
            _userProfileService = userProfileService;
            _kdomReadService = kdomReadService;
        }

        /// <summary>
        /// Obține lista paginată a utilizatorilor (admin only)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserFilterDto filter)
        {
            try
            {
                var result = await _userAdminService.GetAllPaginatedAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține profilul unui utilizator specific (admin only)
        /// </summary>
        [HttpGet("{userId}/profile")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            try
            {
                var profile = await _userProfileService.GetUserProfileAsync(userId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Schimbă rolul unui utilizator (admin only)
        /// </summary>
        [HttpPatch("{userId}/role")]
        public async Task<IActionResult> ChangeUserRole(int userId, [FromBody] ChangeUserRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _userAdminService.ChangeUserRoleAsync(userId, dto.NewRole, adminUserId);
                return Ok(new { message = "User's role has been updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține K-Dom-urile create de un utilizator specific (admin only)
        /// </summary>
        [HttpGet("{userId}/kdoms")]
        public async Task<IActionResult> GetUserKdoms(int userId)
        {
            try
            {
                var result = await _kdomReadService.GetKdomsForUserAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Actualizează profilul unui utilizator specific (admin only)
        /// Folosit pentru moderare sau corecții administrative
        /// </summary>
        [HttpPut("{userId}/profile")]
        public async Task<IActionResult> UpdateUserProfile(int userId, [FromBody] UserProfileUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _userProfileService.UpdateProfileAsync(userId, dto);
                return Ok(new { message = $"Profile for user {userId} has been updated by admin." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{userId}/private")]
        public async Task<IActionResult> GetUserPrivateInfo(int userId)
        {
            try
            {
                var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var privateInfo = await _userProfileService.GetUserPrivateInfoAsync(userId, adminUserId);
                return Ok(privateInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{userId}/detailed-stats")]
        public async Task<IActionResult> GetUserDetailedStats(int userId)
        {
            try
            {
                var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var stats = await _userProfileService.GetUserDetailedStatsAsync(userId, adminUserId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}