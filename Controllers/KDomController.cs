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

        public KDomController(
            IKDomReadService kdomReadService,
            ICollaborationRequestService collaborationRequestService,
            IKDomFollowService kdomFollowService,
            IKDomFlowService kdomFlowService,
            IKDomPermissionService kdomPermissionService
            )
        {
            _kdomReadService = kdomReadService;
            _collaborationRequestService = collaborationRequestService;
            _kdomFollowService = kdomFollowService;
            _kdomFlowService = kdomFlowService;
            _kdomPermissionService = kdomPermissionService;
        }

        #region K-Dom CRUD Operations

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

        // SLUG-BASED EDIT (Primary route for frontend)
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

        // ID-BASED EDIT (Backward compatibility)
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

        #endregion

        #region Metadata Updates

        // SLUG-BASED METADATA UPDATE (Primary route for frontend)
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

        // ID-BASED METADATA UPDATE (Backward compatibility)
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

        #endregion

        #region Read Operations

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

        #endregion

        #region Edit History

        // SLUG-BASED EDIT HISTORY (Primary route for frontend)
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

        // ID-BASED EDIT HISTORY (Backward compatibility)
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

        // SLUG-BASED METADATA HISTORY (Primary route for frontend)
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

        // ID-BASED METADATA HISTORY (Backward compatibility)
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

        #endregion

        #region Admin/Moderator Operations

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

        [Authorize(Roles = "admin,moderator")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _kdomFlowService.ApproveKdomAsync(id, userId);
                return Ok(new { message = "K-Dom approved." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "admin,moderator")]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(string id, [FromBody] KDomRejectDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _kdomFlowService.RejectKdomAsync(id, dto, userId);
                return Ok(new { message = "K-Dom rejected." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Utility Operations

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

        #endregion

        #region Hierarchical Operations

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

        #endregion

        #region Collaboration Operations

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
        [HttpGet("{id}/collaborators")]
        public async Task<IActionResult> GetCollaborators(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var collaborators = await _kdomReadService.GetCollaboratorsAsync(id, userId);
                return Ok(collaborators);
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

        #endregion

        #region Follow Operations

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

        #endregion

        #region Trending and Suggestions

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

        /// <summary>
        /// Verifică și validează un titlu de K-Dom
        /// Versiune îmbunătățită a endpoint-ului /check
        /// </summary>
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

        /// <summary>
        /// Verifică permisiunile utilizatorului pentru un K-Dom specific
        /// Util pentru frontend să determine ce acțiuni poate face utilizatorul
        /// </summary>
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

        /// <summary>
        /// Verifică permisiunile utilizatorului pentru un K-Dom prin slug
        /// </summary>
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


        /// <summary>
        /// Obține statistici detaliate pentru un K-Dom
        /// Include followers, comments, views, last activity
        /// </summary>
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

        /// <summary>
        /// Bulk approve multiple K-Doms (Admin only)
        /// </summary>
        [Authorize(Roles = "admin,moderator")]
        [HttpPost("bulk-approve")]
        public async Task<IActionResult> BulkApprove([FromBody] BulkOperationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var successCount = 0;
                var errors = new List<string>();

                foreach (var kdomId in dto.KDomIds)
                {
                    try
                    {
                        await _kdomFlowService.ApproveKdomAsync(kdomId, userId);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to approve {kdomId}: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    message = $"Bulk operation completed. {successCount}/{dto.KDomIds.Count} K-Doms approved.",
                    successCount,
                    totalCount = dto.KDomIds.Count,
                    errors = errors.Any() ? errors : null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Bulk reject multiple K-Doms (Admin only)
        /// </summary>
        [Authorize(Roles = "admin,moderator")]
        [HttpPost("bulk-reject")]
        public async Task<IActionResult> BulkReject([FromBody] BulkRejectDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var successCount = 0;
                var errors = new List<string>();

                var rejectDto = new KDomRejectDto { Reason = dto.Reason };

                foreach (var kdomId in dto.KDomIds)
                {
                    try
                    {
                        await _kdomFlowService.RejectKdomAsync(kdomId, rejectDto, userId);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to reject {kdomId}: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    message = $"Bulk operation completed. {successCount}/{dto.KDomIds.Count} K-Doms rejected.",
                    successCount,
                    totalCount = dto.KDomIds.Count,
                    errors = errors.Any() ? errors : null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține sugestii de K-Dom-uri similare pe baza unui titlu
        /// Util pentru evitarea duplicatelor și ghidarea utilizatorilor
        /// </summary>
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

        /// <summary>
        /// Verifică dacă un utilizator poate crea un sub-K-Dom pentru un parent
        /// </summary>
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

        #endregion
    }
}