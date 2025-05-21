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
        private readonly IKDomService _kdomService;
        private readonly IKDomRepository _repository;
        private readonly ICollaborationRequestService _collaborationRequestService;
        public KDomController(
            IKDomService kdomService, 
            IKDomRepository repository, 
            ICollaborationRequestService collaborationRequestService)
        {
            _kdomService = kdomService;
            _repository = repository;
            _collaborationRequestService = collaborationRequestService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] KDomCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _kdomService.CreateKDomAsync(dto, userId);
            return Ok(new { message = "KDom created successfully." });
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

            var changed = await _kdomService.EditKDomAsync(dto, userId);

            if (!changed)
                return NoContent(); // 204: nimic de salvat

            return Ok(new { message = "KDom updated successfully." });
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
                var changed = await _kdomService.UpdateKDomMetadataAsync(dto, userId);
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
                var dto = await _kdomService.GetKDomByIdAsync(id);
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
                var edits = await _kdomService.GetEditHistoryAsync(id, userId);
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
                var history = await _kdomService.GetMetadataEditHistoryAsync(id, userId);
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
            var result = await _kdomService.GetPendingKdomsAsync();
            return Ok(result);
        }

        [Authorize(Roles = "admin,moderator")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _kdomService.ApproveKdomAsync(id, userId);
            return Ok(new { message = "K-Dom approved." });
        }

        [Authorize(Roles = "admin,moderator")]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(string id, [FromBody] KDomRejectDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _kdomService.RejectKdomAsync(id, dto, userId);
            return Ok(new { message = "K-Dom rejected." });
        }
        [HttpGet("check")]
        public async Task<IActionResult> CheckTitle([FromQuery] string title)
        {
            var slug = SlugHelper.GenerateSlug(title); 

            var exists = await _repository.ExistsByTitleOrSlugAsync(title, slug);
            var suggestions = await _repository.FindSimilarByTitleAsync(title);

            return Ok(new
            {
                exists,
                suggestions = suggestions.Select(k => k.Title).ToList()
            });
        }

        [HttpGet("{id}/children")]
        public async Task<IActionResult> GetChildren(string id)
        {
            var children = await _kdomService.GetChildrenAsync(id);
            return Ok(children);
        }

        [HttpGet("{id}/parent")]
        public async Task<IActionResult> GetParent(string id)
        {
            var parent = await _kdomService.GetParentAsync(id);
            if (parent == null)
                return NotFound(new { message = "This K-Dom does not have a parent K-Dom." });

            return Ok(parent);
        }

        [HttpGet("{id}/related")]
        public async Task<IActionResult> GetRelated(string id)
        {
            var siblings = await _kdomService.GetSiblingsAsync(id);
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
                var collaborators = await _kdomService.GetCollaboratorsAsync(id, userId);
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
                await _kdomService.RemoveCollaboratorAsync(id, userId, userIdToRemove);
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


    }
}
