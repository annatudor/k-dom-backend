using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;
using System.Security.Claims;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Collaboration;

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
        public KDomController(
            IKDomReadService kdomReadService, 
            ICollaborationRequestService collaborationRequestService,
            IKDomFollowService kdomFollowService,
            IKDomFlowService kdomFlowService
            )
        {
            _kdomReadService = kdomReadService;
            _collaborationRequestService = collaborationRequestService;
            _kdomFollowService = kdomFollowService;
            _kdomFlowService = kdomFlowService;

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] KDomCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _kdomFlowService.CreateKDomAsync(dto, userId);
            return Ok(new { message = "K-Dom created successfully." });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] KDomEditDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.KDomId != id)
                return BadRequest("ID mismatch.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var changed = await _kdomFlowService.EditKDomAsync(dto, userId);

            if (!changed)
                return NoContent(); // 204: nimic de salvat

            return Ok(new { message = "K-Dom updated successfully." });
        }


        [Authorize]
        [HttpPut("{id}/metadata")]
        public async Task<IActionResult> UpdateMetadata(string id, [FromBody] KDomUpdateMetadataDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.KDomId != id)
                return BadRequest("ID mismatch.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var changed = await _kdomFlowService.UpdateKDomMetadataAsync(dto, userId);
                if (!changed)
                    return NoContent();

                return Ok(new { message = "Metadata updated successfully." });
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
        [Authorize]
        [HttpGet("{id}/edits")]
        public async Task<IActionResult> GetEditHistory(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var edits = await _kdomReadService.GetEditHistoryAsync(id, userId);
                return Ok(edits);
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
        [HttpGet("{id}/metadata-history")]
        public async Task<IActionResult> GetMetadataHistory(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var history = await _kdomReadService.GetMetadataEditHistoryAsync(id, userId);
                return Ok(history);
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

        [Authorize(Roles = "admin,moderator")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _kdomReadService.GetPendingKdomsAsync();
            return Ok(result);
        }

        [Authorize(Roles = "admin,moderator")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _kdomFlowService.ApproveKdomAsync(id, userId);
            return Ok(new { message = "K-Dom approved." });
        }

        [Authorize(Roles = "admin,moderator")]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(string id, [FromBody] KDomRejectDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _kdomFlowService.RejectKdomAsync(id, dto, userId);
            return Ok(new { message = "K-Dom rejected." });
        }
       
        [HttpGet("check")]
        public async Task<IActionResult> CheckTitle([FromQuery] string title)
        {
            var exists = await _kdomReadService.ExistsByTitleOrSlugAsync(title);
            var suggestions = await _kdomReadService.GetSimilarTitlesAsync(title);

            return Ok(new
            {
                exists,
                suggestions
            });
        }


        [HttpGet("{id}/children")]
        public async Task<IActionResult> GetChildren(string id)
        {
            var children = await _kdomReadService.GetChildrenAsync(id);
            return Ok(children);
        }

        [HttpGet("{id}/parent")]
        public async Task<IActionResult> GetParent(string id)
        {
            var parent = await _kdomReadService.GetParentAsync(id);
            if (parent == null)
                return NotFound(new { message = "This K-Dom does not have a parent K-Dom." });

            return Ok(parent);
        }

        [HttpGet("{id}/related")]
        public async Task<IActionResult> GetRelated(string id)
        {
            var siblings = await _kdomReadService.GetSiblingsAsync(id);
            return Ok(siblings);
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


        [Authorize]
        [HttpPost("{id}/collab-requests")]
        public async Task<IActionResult> RequestCollaboration(string id, [FromBody] CollaborationRequestCreateDto dto)
        {
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
            await _collaborationRequestService.ApproveAsync(kdomId, requestId, userId);
            return Ok(new { message = "Request approved." });
        }

        [Authorize]
        [HttpPost("{kdomId}/collab-requests/{requestId}/reject")]
        public async Task<IActionResult> RejectRequest(string kdomId, string requestId, [FromBody] CollaborationRequestActionDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _collaborationRequestService.RejectAsync(kdomId, requestId, userId, dto.RejectionReason);
            return Ok(new { message = "Request rejected." });
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
            catch (UnauthorizedAccessException)
            {
                return Forbid();
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
            catch (UnauthorizedAccessException)
            {
                return Forbid();
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
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("{parentId}/sub")]
        public async Task<IActionResult> CreateSubKDom(string parentId, [FromBody] KDomSubCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _kdomFlowService.CreateSubKDomAsync(parentId, dto, userId);
                return Ok(new { message = "SubK-Dom created succesfully." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("search-tag-slug")]
        public async Task<IActionResult> SearchTagOrSlug([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { error = "'query' field is required." });

            var results = await _kdomReadService.SearchTagOrSlugAsync(query.Trim());
            return Ok(results);
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
            var followed = await _kdomFollowService.GetFollowedKDomsAsync(userId);
            return Ok(followed);
        }

        [Authorize]
        [HttpGet("{id}/is-followed")]
        public async Task<IActionResult> IsFollowed(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _kdomFollowService.IsFollowingAsync(id, userId);
            return Ok(new { isFollowed = result });
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingKdoms([FromQuery] int days = 7)
        {
            var trending = await _kdomReadService.GetTrendingKdomsAsync(days);
            return Ok(trending.OrderByDescending(t => t.TotalScore));
        }

        [Authorize]
        [HttpGet("suggested")]
        public async Task<IActionResult> GetSuggestedKdoms()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _kdomReadService.GetSuggestedKdomsAsync(userId);
            return Ok(result);
        }

        [HttpGet("{id}/followers/count")]
        public async Task<IActionResult> GetKDomFollowersCount(string id)
        {
            var count = await _kdomFollowService.GetFollowersCountAsync(id);
            return Ok(new { count });
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            try
            {
                var dto = await _kdomReadService.GetKDomBySlugAsync(slug);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }


    }
}
