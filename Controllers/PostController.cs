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
        private readonly IPostReadService _postReadService;
        private readonly IPostFlowService _postFlowService;

        public PostController(IPostReadService postReadService, IPostFlowService postFlowService)
        {
            _postReadService = postReadService;
            _postFlowService = postFlowService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PostCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _postFlowService.CreatePostAsync(dto, userId);
            return Ok(new { message = "Post created successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _postReadService.GetAllPostsAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var post = await _postReadService.GetByIdAsync(id);
                return Ok(post);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        // FIXED: Changed from PUT to POST and fixed route
        [Authorize]
        [HttpPost("{id}/like")]
        public async Task<IActionResult> ToggleLike(string id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _postFlowService.ToggleLikeAsync(id, userId);
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
                await _postFlowService.EditPostAsync(id, dto, userId);
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
                await _postFlowService.DeletePostAsync(id, userId, isModerator);
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
            var posts = await _postReadService.GetPostsByUserIdAsync(userId);
            return Ok(posts);
        }

        [Authorize]
        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var posts = await _postReadService.GetFeedAsync(userId);

            if (!posts.Any())
            {
                return Ok(new
                {
                    message = "You're not following anyone yet. Follow users or K-Doms to see personalized posts here.",
                    posts = new List<PostReadDto>()
                });
            }

            return Ok(posts);
        }

        [AllowAnonymous]
        [HttpGet("guest-feed")]
        public async Task<IActionResult> GetGuestFeed([FromQuery] int limit = 30)
        {
            var posts = await _postReadService.GetGuestFeedAsync(limit);
            return Ok(posts);
        }

        [HttpGet("by-tag")]
        public async Task<IActionResult> GetByTag(
        [FromQuery] string tag,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return BadRequest(new { error = "Tag is required." });

            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20; // Limitează la max 100 pentru performanță

            try
            {
                var pagedPosts = await _postReadService.GetPostsByTagAsync(tag.ToLower(), page, pageSize);

                return Ok(new
                {
                    tag = tag,
                    posts = pagedPosts.Items,
                    pagination = new
                    {
                        currentPage = pagedPosts.CurrentPage,
                        totalPages = pagedPosts.TotalPages,
                        totalItems = pagedPosts.TotalCount,
                        pageSize = pagedPosts.PageSize,
                        hasNextPage = pagedPosts.CurrentPage < pagedPosts.TotalPages,
                        hasPreviousPage = pagedPosts.CurrentPage > 1
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}