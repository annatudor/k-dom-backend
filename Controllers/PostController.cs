using KDomBackend.Models.DTOs.Post;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PostCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _postService.CreatePostAsync(dto, userId);
            return Ok(new { message = "Post created successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _postService.GetAllPostsAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var post = await _postService.GetByIdAsync(id);
                return Ok(post);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }


        [Authorize]
        [HttpPut("{id}/like")]
        public async Task<IActionResult> ToggleLike(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var result = await _postService.ToggleLikeAsync(id, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] PostEditDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _postService.EditPostAsync(id, dto, userId);
                return Ok(new { message = "Post updated successfully." });
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
                await _postService.DeletePostAsync(id, userId, isModerator);
                return Ok(new { message = "Your post has been deleted." });
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


        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var posts = await _postService.GetPostsByUserIdAsync(userId);
            return Ok(posts);
        }

        [Authorize]
        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var posts = await _postService.GetFeedAsync(userId);

            if (!posts.Any())
            {
                return Ok(new
                {
                    message = "You’re not following anyone yet. Follow users or K-Doms to see personalized posts here.",
                    posts = new List<PostReadDto>()
                });
            }

            return Ok(posts);
        }


        [AllowAnonymous]
        [HttpGet("guest-feed")]
        public async Task<IActionResult> GetGuestFeed([FromQuery] int limit = 30)
        {
            var posts = await _postService.GetGuestFeedAsync(limit);
            return Ok(posts);
        }

        [HttpGet("by-tag")]
        public async Task<IActionResult> GetByTag([FromQuery] string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return BadRequest(new { error = "Tag is required." });

            var posts = await _postService.GetPostsByTagAsync(tag.ToLower());
            return Ok(posts);
        }


    }
}
