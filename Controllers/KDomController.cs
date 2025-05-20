using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/kdoms")]
    public class KDomController : ControllerBase
    {
        private readonly IKDomService _kdomService;

        public KDomController(IKDomService kdomService)
        {
            _kdomService = kdomService;
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


    }
}
