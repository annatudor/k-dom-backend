using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Comment;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KDomBackend.Controllers
{
    [Authorize]
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

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] CommentTargetType targetType, [FromQuery] string targetId)
        {
            var comments = await _commentService.GetCommentsByTargetAsync(targetType, targetId);
            return Ok(comments);
        }


        [HttpGet("{id}/replies")]
        public async Task<IActionResult> GetReplies(string id)
        {
            var replies = await _commentService.GetRepliesAsync(id);
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



    }
}
