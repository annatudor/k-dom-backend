using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class PublicUserController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IKDomReadService _kdomReadService;

        public PublicUserController(
            IUserProfileService userProfileService,
            IKDomReadService kdomReadService)
        {
            _userProfileService = userProfileService;
            _kdomReadService = kdomReadService;
        }

        /// <summary>
        /// Obține profilul public al unui utilizator
        /// Poate fi accesat de oricine pentru a vedea profilurile publice
        /// </summary>
        [HttpGet("{userId}/profile")]
        [AllowAnonymous]
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
        /// Obține K-Dom-urile publice create de un utilizator
        /// Poate fi accesat de oricine
        /// </summary>
        [HttpGet("{userId}/kdoms")]
        [AllowAnonymous]
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
    }
}