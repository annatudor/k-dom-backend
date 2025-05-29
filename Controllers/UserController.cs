using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KDomBackend.Services.Interfaces;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Enums;
using KDomBackend.Services.Implementations;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IUserAdminService _userAdminService;
        private readonly IKDomReadService _kdomReadService;

        public UserController(IUserService userService, IKDomReadService kdomReadService, IUserProfileService userProfileService, IUserAdminService userAdminService )
        {
            _userProfileService = userProfileService;
            _userAdminService = userAdminService;
            _kdomReadService = kdomReadService;
        }

        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfile(int id)
        {
            try
            {
                var profile = await _userProfileService.GetUserProfileAsync(id);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _userProfileService.UpdateProfileAsync(userId, dto);
                return Ok(new { message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPatch("{id}/role")]
        public async Task<IActionResult> ChangeUserRole(int id, [FromBody] ChangeUserRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _userAdminService.ChangeUserRoleAsync(id, dto.NewRole, adminUserId);
                return Ok(new { message = "User's role has been updated." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserFilterDto filter)
        {
            var result = await _userAdminService.GetAllPaginatedAsync(filter);
            return Ok(result);
        }

        [HttpGet("profile/themes")]
        public IActionResult GetProfileThemes()
        {
            var themes = Enum.GetNames(typeof(ProfileTheme));
            return Ok(themes);
        }

        [HttpGet("{id}/kdoms")]
        public async Task<IActionResult> GetKdomsForUser(int id)
        {
            var result = await _kdomReadService.GetKdomsForUserAsync(id);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("recently-viewed-kdoms")]
        public async Task<IActionResult> GetRecentlyViewedKdoms()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _kdomReadService.GetRecentlyViewedKdomsAsync(userId);
            return Ok(result);
        }

    }
}
