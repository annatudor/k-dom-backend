using KDomBackend.Models.DTOs.User;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = await _authService.RegisterUserAsync(dto);
                return CreatedAtAction(nameof(Register), new { id = userId }, new { message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var token = await _authService.AuthenticateAsync(dto);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Received DTO - CurrentPassword: '{dto?.CurrentPassword}', NewPassword: '{dto?.NewPassword}'");
                Console.WriteLine($"[DEBUG] CurrentPassword is null: {dto?.CurrentPassword == null}");
                Console.WriteLine($"[DEBUG] CurrentPassword is empty: {string.IsNullOrEmpty(dto?.CurrentPassword)}");
                Console.WriteLine($"[DEBUG] ModelState.IsValid: {ModelState.IsValid}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("[DEBUG] ModelState errors:");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"[DEBUG] Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                    return BadRequest(ModelState);
                }

                if (dto == null)
                {
                    Console.WriteLine("[DEBUG] DTO is null!");
                    return BadRequest(new { error = "Invalid request data" });
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                Console.WriteLine($"[DEBUG] User ID from claims: {userId}");

                await _authService.ChangePasswordAsync(userId, dto);
                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Exception in ChangePassword: {ex.Message}");
                Console.WriteLine($"[DEBUG] Stack trace: {ex.StackTrace}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _authService.RequestPasswordResetAsync(dto);
                return Ok(new { message = "If this email exists, a reset link has been sent." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            Console.WriteLine("[DEBUG] ResetPassword endpoint hit.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _authService.ResetPasswordAsync(dto);
                return Ok(new { message = "Password reset successful." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


    }
}
