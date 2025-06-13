using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;
using System.Security.Claims;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Collaboration;
using KDomBackend.Services.Implementations;
using KDomBackend.Models.DTOs.Common;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/kdoms")]
    public class KDomController : ControllerBase
    {
        private readonly IKDomReadService _kdomReadService;
        private readonly ICollaborationRequestService _collaborationRequestService;
        private readonly IKDomFollowService _kdomFollowService;
        private readonly IKDomFlowService _kdomFlowService;
        private readonly IKDomPermissionService _kdomPermissionService;
        private readonly IPostReadService _postReadService;
        private readonly IKDomDiscussionService _kdomDiscussionService;

        public KDomController(
            IKDomReadService kdomReadService,
            ICollaborationRequestService collaborationRequestService,
            IKDomFollowService kdomFollowService,
            IKDomFlowService kdomFlowService,
            IKDomPermissionService kdomPermissionService,
            IPostReadService postReadService,
            IKDomDiscussionService kdomDiscussionService
            )
        {
            _kdomReadService = kdomReadService;
            _collaborationRequestService = collaborationRequestService;
            _kdomFollowService = kdomFollowService;
            _kdomFlowService = kdomFlowService;
            _kdomPermissionService = kdomPermissionService;
            _postReadService = postReadService;
            _kdomDiscussionService = kdomDiscussionService;
        }



        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] KDomCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _kdomFlowService.CreateKDomAsync(dto, userId);
                return Ok(new { message = "K-Dom created successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [Authorize]
        [HttpPut("slug/{slug}")]
        public async Task<IActionResult> EditBySlug(string slug, [FromBody] KDomEditDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.KDomSlug != slug)
                return BadRequest("Slug mismatch.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var changed = await _kdomFlowService.EditKDomBySlugAsync(dto, userId);

                if (!changed)
                    return NoContent(); // 204: nothing to save

                return Ok(new { message = "K-Dom updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditById(string id, [FromBody] KDomEditDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.KDomSlug != id)
                return BadRequest("ID mismatch.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var changed = await _kdomFlowService.EditKDomAsync(dto, userId);

                if (!changed)
                    return NoContent(); // 204: nothing to save

                return Ok(new { message = "K-Dom updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

 
        [Authorize]
        [HttpPut("slug/{slug}/metadata")]
        public async Task<IActionResult> UpdateMetadataBySlug(string slug, [FromBody] KDomUpdateMetadataDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                // NU mai verificăm slug mismatch - slug-ul vine din URL, nu din body
                var changed = await _kdomFlowService.UpdateKDomMetadataBySlugAsync(slug, dto, userId);
                if (!changed)
                    return NoContent();

                return Ok(new { message = "Metadata updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/metadata")]
        public async Task<IActionResult> UpdateMetadataById(string id, [FromBody] KDomUpdateMetadataDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                // NU mai verificăm ID mismatch - ID-ul vine din URL, nu din body
                var changed = await _kdomFlowService.UpdateKDomMetadataByIdAsync(id, dto, userId);
                if (!changed)
                    return NoContent();

                return Ok(new { message = "Metadata updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var dto = await _kdomReadService.GetKDomByIdAsync(id);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            try
            {
                var kdom = await _kdomReadService.GetKDomBySlugAsync(slug);
                return Ok(kdom);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

     

        [Authorize]
        [HttpGet("slug/{slug}/edits")]
        public async Task<IActionResult> GetEditHistoryBySlug(string slug)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var edits = await _kdomReadService.GetEditHistoryBySlugAsync(slug, userId);
                return Ok(edits);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }


        [Authorize]
        [HttpGet("{id}/edits")]
        public async Task<IActionResult> GetEditHistoryById(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var edits = await _kdomReadService.GetEditHistoryAsync(id, userId);
                return Ok(edits);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

 
        [Authorize]
        [HttpGet("slug/{slug}/metadata-history")]
        public async Task<IActionResult> GetMetadataHistoryBySlug(string slug)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var history = await _kdomReadService.GetMetadataEditHistoryBySlugAsync(slug, userId);
                return Ok(history);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{id}/metadata-history")]
        public async Task<IActionResult> GetMetadataHistoryById(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var history = await _kdomReadService.GetMetadataEditHistoryAsync(id, userId);
                return Ok(history);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

   

        [Authorize(Roles = "admin,moderator")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            try
            {
                var result = await _kdomReadService.GetPendingKdomsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("check")]
        public async Task<IActionResult> CheckTitle([FromQuery] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest(new { error = "Title is required." });

            try
            {
                var exists = await _kdomReadService.ExistsByTitleOrSlugAsync(title);
                var suggestions = await _kdomReadService.GetSimilarTitlesAsync(title);

                return Ok(new
                {
                    exists,
                    suggestions
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("languages")]
        public IActionResult GetAvailableLanguages()
        {
            var values = Enum.GetNames(typeof(Language));
            return Ok(values);
        }

        [HttpGet("hubs")]
        public IActionResult GetAvailableHubs()
        {
            var values = Enum.GetNames(typeof(Hub));
            return Ok(values);
        }

        [HttpGet("themes")]
        public IActionResult GetAvailableThemes()
        {
            var themes = Enum.GetNames(typeof(KDomTheme));
            return Ok(themes);
        }

        [HttpGet("search-tag-slug")]
        public async Task<IActionResult> SearchTagOrSlug([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { error = "'query' field is required." });

            try
            {
                var results = await _kdomReadService.SearchTagOrSlugAsync(query.Trim());
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("{id}/children")]
        public async Task<IActionResult> GetChildren(string id)
        {
            try
            {
                var children = await _kdomReadService.GetChildrenAsync(id);
                return Ok(children);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("{id}/parent")]
        public async Task<IActionResult> GetParent(string id)
        {
            try
            {
                var parent = await _kdomReadService.GetParentAsync(id);
                if (parent == null)
                    return NotFound(new { message = "This K-Dom does not have a parent K-Dom." });

                return Ok(parent);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("{id}/related")]
        public async Task<IActionResult> GetRelated(string id)
        {
            try
            {
                var siblings = await _kdomReadService.GetSiblingsAsync(id);
                return Ok(siblings);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("{parentId}/sub")]
        public async Task<IActionResult> CreateSubKDom(string parentId, [FromBody] KDomSubCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _kdomFlowService.CreateSubKDomAsync(parentId, dto, userId);
                return Ok(new { message = "Sub K-Dom created successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [Authorize]
        [HttpPost("{id}/collab-requests")]
        public async Task<IActionResult> RequestCollaboration(string id, [FromBody] CollaborationRequestCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _collaborationRequestService.CreateRequestAsync(id, userId, dto);
                return Ok(new { message = "Collaboration request submitted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("{kdomId}/collab-requests/{requestId}/approve")]
        public async Task<IActionResult> ApproveRequest(string kdomId, string requestId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _collaborationRequestService.ApproveAsync(kdomId, requestId, userId);
                return Ok(new { message = "Request approved." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("{kdomId}/collab-requests/{requestId}/reject")]
        public async Task<IActionResult> RejectRequest(string kdomId, string requestId, [FromBody] CollaborationRequestActionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _collaborationRequestService.RejectAsync(kdomId, requestId, userId, dto.RejectionReason);
                return Ok(new { message = "Request rejected." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{kdomId}/collab-requests")]
        public async Task<IActionResult> GetCollabRequests(string kdomId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var result = await _collaborationRequestService.GetRequestsAsync(kdomId, userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [Authorize]
        [HttpDelete("{id}/collaborators/{userIdToRemove}")]
        public async Task<IActionResult> RemoveCollaborator(string id, int userIdToRemove)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _kdomFlowService.RemoveCollaboratorAsync(id, userId, userIdToRemove);
                return Ok(new { message = "Collaborator removed." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

     

        [Authorize]
        [HttpPost("{id}/follow")]
        public async Task<IActionResult> FollowKDom(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _kdomFollowService.FollowAsync(id, userId);
                return Ok(new { message = "You are now following this K-Dom." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{id}/unfollow")]
        public async Task<IActionResult> UnfollowKDom(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _kdomFollowService.UnfollowAsync(id, userId);
                return Ok(new { message = "You unfollowed this K-Dom." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("followed")]
        public async Task<IActionResult> GetFollowedKdoms()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var followed = await _kdomFollowService.GetFollowedKDomsAsync(userId);
                return Ok(followed);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{id}/is-followed")]
        public async Task<IActionResult> IsFollowed(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var result = await _kdomFollowService.IsFollowingAsync(id, userId);
                return Ok(new { isFollowed = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}/followers/count")]
        public async Task<IActionResult> GetKDomFollowersCount(string id)
        {
            try
            {
                var count = await _kdomFollowService.GetFollowersCountAsync(id);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

 

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingKdoms([FromQuery] int days = 7)
        {
            try
            {
                var trending = await _kdomReadService.GetTrendingKdomsAsync(days);
                return Ok(trending.OrderByDescending(t => t.TotalScore));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("suggested")]
        public async Task<IActionResult> GetSuggestedKdoms()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var result = await _kdomReadService.GetSuggestedKdomsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

       
        [HttpPost("validate-title")]
        public async Task<IActionResult> ValidateTitle([FromBody] ValidateTitleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var exists = await _kdomReadService.ExistsByTitleOrSlugAsync(dto.Title);
                var suggestions = exists ? await _kdomReadService.GetSimilarTitlesAsync(dto.Title) : new List<string>();

                return Ok(new
                {
                    exists,
                    suggestions,
                    isValid = !exists,
                    message = exists ? "Title already exists. Try one of the suggestions or create a different title." : "Title is available!",
                    suggestedAlternatives = suggestions.Take(3).ToList() // Limitează la 3 sugestii
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{id}/permissions")]
        public async Task<IActionResult> GetUserPermissions(string id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var permissions = await _kdomPermissionService.GetUserPermissionsByIdAsync(id, userId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("slug/{slug}/permissions")]
        public async Task<IActionResult> GetUserPermissionsBySlug(string slug)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var permissions = await _kdomPermissionService.GetUserPermissionsBySlugAsync(slug, userId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


    
        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetKDomStats(string id)
        {
            try
            {
                var followersCount = await _kdomFollowService.GetFollowersCountAsync(id);

                // Aici ai putea adăuga și alte statistici când implementezi view tracking
                return Ok(new KDomStatsDto
                {
                    FollowersCount = followersCount,
                    ViewsCount = 0, // Placeholder până implementezi view tracking
                    CommentsCount = 0, // Ar trebui calculat prin comment service
                    EditsCount = 0, // Ar trebui calculat prin edit history
                    LastActivity = DateTime.UtcNow // Placeholder
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


      
        [HttpGet("suggest-similar")]
        public async Task<IActionResult> GetSimilarSuggestions([FromQuery] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest(new { error = "Title parameter is required." });

            try
            {
                var suggestions = await _kdomReadService.GetSimilarTitlesAsync(title);
                var searchResults = await _kdomReadService.SearchTagOrSlugAsync(title);

                return Ok(new
                {
                    similarTitles = suggestions,
                    relatedKDoms = searchResults.Take(5), // Limitează la 5 rezultate
                    message = suggestions.Any() ? "Found similar K-Doms" : "No similar K-Doms found"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    
        [Authorize]
        [HttpGet("{parentId}/can-create-sub")]
        public async Task<IActionResult> CanCreateSubKDom(string parentId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var canCreate = await _kdomPermissionService.CanUserCreateSubKDomByIdAsync(parentId, userId);

                return Ok(new
                {
                    canCreate,
                    message = canCreate ? "You can create a sub-page for this K-Dom." : "You don't have permission to create sub-pages for this K-Dom."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

  

        [HttpGet("slug/{slug}/discussion")]
        public async Task<IActionResult> GetKDomDiscussion(string slug, [FromQuery] KDomDiscussionFilterDto filter)
        {
            try
            {
             
                Console.WriteLine($"[KDomController] GetKDomDiscussion called with slug: {slug}");
                Console.WriteLine($"[KDomController] Filter: Page={filter.Page}, PageSize={filter.PageSize}");

                var discussion = await _kdomDiscussionService.GetKDomDiscussionAsync(slug, filter);

               
                Console.WriteLine($"[KDomController] Discussion result:");
                Console.WriteLine($"  - KDom ID: {discussion.KDom?.Id}");
                Console.WriteLine($"  - KDom Title: {discussion.KDom?.Title}");
                Console.WriteLine($"  - KDom Author: {discussion.KDom?.AuthorUsername}");
                Console.WriteLine($"  - KDom Followers: {discussion.KDom?.FollowersCount}");
                Console.WriteLine($"  - Posts Count: {discussion.Posts?.Items?.Count}");
                Console.WriteLine($"  - Stats Total Posts: {discussion.Stats?.TotalPosts}");

             
                if (discussion.KDom == null)
                {
                    Console.WriteLine("[KDomController] WARNING: KDom object is null!");
                }

                if (string.IsNullOrEmpty(discussion.KDom?.AuthorUsername))
                {
                    Console.WriteLine("[KDomController] WARNING: AuthorUsername is missing!");
                }

                var response = new
                {
                    kdom = discussion.KDom,
                    posts = discussion.Posts,
                    stats = discussion.Stats
                };

      
                Console.WriteLine($"[KDomController] Returning response structure:");
                Console.WriteLine($"  - response.kdom is null: {response.kdom == null}");
                Console.WriteLine($"  - response.posts is null: {response.posts == null}");
                Console.WriteLine($"  - response.stats is null: {response.stats == null}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KDomController] Error in GetKDomDiscussion: {ex.Message}");
                Console.WriteLine($"[KDomController] Stack trace: {ex.StackTrace}");
                return NotFound(new { error = ex.Message });
            }
        }

    
        [HttpGet("slug/{slug}/discussion/stats")]
        public async Task<IActionResult> GetDiscussionStats(string slug)
        {
            try
            {
                var stats = await _kdomDiscussionService.GetDiscussionStatsAsync(slug);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("slug/{slug}/has-discussion")]
        public async Task<IActionResult> HasActiveDiscussion(string slug)
        {
            try
            {
                var hasDiscussion = await _kdomDiscussionService.HasActiveDiscussionAsync(slug);
                return Ok(new
                {
                    slug,
                    hasActiveDiscussion = hasDiscussion,
                    message = hasDiscussion ? "This K-Dom has active discussions" : "No discussions yet for this K-Dom"
                });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("slug/{slug}/posts")]
        public async Task<IActionResult> GetKDomPosts(string slug, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                // Verifică că K-Dom-ul există
                var kdom = await _kdomReadService.GetKDomBySlugAsync(slug);

                // Obține postările cu paginare
                var pagedPosts = await _postReadService.GetPostsByTagAsync(slug.ToLower(), page, pageSize);

                return Ok(new
                {
                    kdom = new
                    {
                        id = kdom.Id,
                        title = kdom.Title,
                        slug = kdom.Slug,
                        description = kdom.Description
                    },
                    posts = pagedPosts,
                    pagination = new
                    {
                        currentPage = pagedPosts.CurrentPage,
                        totalPages = pagedPosts.TotalPages,
                        totalItems = pagedPosts.TotalCount,
                        pageSize = pagedPosts.PageSize
                    }
                });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("slug/{slug}/discussion/search")]
        public async Task<IActionResult> SearchKDomDiscussion(string slug, [FromQuery] KDomDiscussionSearchDto searchDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchDto.ContentQuery) &&
                    string.IsNullOrWhiteSpace(searchDto.Username))
                {
                    return BadRequest(new { error = "At least one search parameter (contentQuery or username) is required." });
                }

                var discussion = await _kdomDiscussionService.SearchKDomDiscussionAsync(slug, searchDto);

                return Ok(new
                {
                    slug,
                    searchParams = new
                    {
                        contentQuery = searchDto.ContentQuery,
                        username = searchDto.Username,
                        sortBy = searchDto.SortBy,
                        onlyLiked = searchDto.OnlyLiked,
                        lastDays = searchDto.LastDays
                    },
                    results = discussion,
                    message = discussion.Posts.Items.Any() ?
                        $"Found {discussion.Posts.TotalCount} posts matching your search" :
                        "No posts found matching your search criteria"
                });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("explore")]
        public async Task<ActionResult<PagedResult<ExploreKDomDto>>> ExploreKDoms([FromQuery] ExploreFilterDto filters)
        {
            var result = await _kdomReadService.GetKDomsForExploreAsync(filters);
            return Ok(result);
        }
    }

}