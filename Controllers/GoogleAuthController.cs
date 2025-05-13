using KDomBackend.Models.DTOs.Auth;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/oauth/google")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly IGoogleAuthService _googleAuthService;

        public GoogleAuthController(IGoogleAuthService googleAuthService)
        {
            _googleAuthService = googleAuthService;
        }

        [HttpPost("callback")]
        public async Task<IActionResult> GoogleCallback([FromBody] GoogleAuthCodeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var jwt = await _googleAuthService.HandleGoogleLoginAsync(dto.Code);
                return Ok(new { token = jwt });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
