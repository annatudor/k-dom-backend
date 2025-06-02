using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Comment;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _commentService.CreateCommentAsync(dto, userId);
            return Ok(new { message = "Comment created successfully." });
        }

        // FIXED: Remove [Authorize] and make this endpoint public
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] CommentTargetType targetType, [FromQuery] string targetId)
        {
            int? currentUserId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            }

            var comments = await _commentService.GetCommentsByTargetAsync(targetType, targetId, currentUserId);
            return Ok(comments);
        }

        // FIXED: Remove [Authorize] and make this endpoint public for replies
        [AllowAnonymous]
        [HttpGet("{id}/replies")]
        public async Task<IActionResult> GetReplies(string id)
        {
            int? currentUserId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            }

            var replies = await _commentService.GetRepliesAsync(id, currentUserId);
            return Ok(replies);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] CommentEditDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _commentService.EditCommentAsync(id, dto, userId);
                return Ok(new { message = "Comment edited successfully." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isModerator = User.IsInRole("admin") || User.IsInRole("moderator");

            try
            {
                await _commentService.DeleteCommentAsync(id, userId, isModerator);
                return Ok(new { message = "Your comment has been deleted." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("{id}/like")]
        public async Task<IActionResult> ToggleLike(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _commentService.ToggleLikeAsync(id, userId);
            return Ok(result);
        }
    }
}