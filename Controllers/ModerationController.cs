using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Models.DTOs.Moderation;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/moderation")]
    [Authorize(Roles = "admin,moderator")]
    public class ModerationController : ControllerBase
    {
        private readonly IModerationService _moderationService;
        public ModerationController(IModerationService moderationService) 
        { 
        _moderationService = moderationService;
        }

        [Authorize(Roles = "admin,moderator")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _moderationService.ApproveKDomAsync(id, userId); 
                return Ok(new { message = "K-Dom approved." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [Authorize(Roles = "admin,moderator")]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(string id, [FromBody] KDomRejectDto dto, [FromQuery] bool deleteAfterReject = true)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                if (deleteAfterReject)
                {
                    await _moderationService.RejectAndDeleteKDomAsync(id, dto.Reason, userId); 
                }
                else
                {
                    await _moderationService.RejectKDomAsync(id, dto.Reason, userId); 
                }

                var message = deleteAfterReject
                    ? "K-Dom rejected and deleted successfully."
                    : "K-Dom rejected successfully.";

                return Ok(new { message, deleted = deleteAfterReject });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}/force-delete")]
        public async Task<IActionResult> ForceDelete(string id, [FromBody] ForceDeleteReasonDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _moderationService.ForceDeleteKDomAsync(id, userId, dto.Reason);

                return Ok(new
                {
                    message = "K-Dom has been force deleted by administrator.",
                    kdomId = id,
                    action = "force_deleted"
                });
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

        // ✅ ACTUALIZEAZĂ - folosește ModerationService pentru bulk reject
        [Authorize(Roles = "admin,moderator")]
        [HttpPost("bulk-reject")]
        public async Task<IActionResult> BulkReject([FromBody] BulkRejectDto dto, [FromQuery] bool deleteAfterReject = true)
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
                        if (deleteAfterReject)
                        {
                            await _moderationService.RejectAndDeleteKDomAsync(kdomId, dto.Reason, userId); // ✅ SCHIMBAT
                        }
                        else
                        {
                            await _moderationService.RejectKDomAsync(kdomId, dto.Reason, userId); // ✅ SCHIMBAT
                        }
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to reject {kdomId}: {ex.Message}");
                    }
                }

                var action = deleteAfterReject ? "rejected and deleted" : "rejected";

                return Ok(new
                {
                    message = $"Bulk operation completed. {successCount}/{dto.KDomIds.Count} K-Doms {action}.",
                    successCount,
                    totalCount = dto.KDomIds.Count,
                    deleted = deleteAfterReject,
                    errors = errors.Any() ? errors : null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

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
                        await _moderationService.ApproveKDomAsync(kdomId, userId);
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

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetModerationDashboard()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var dashboard = await _moderationService.GetModerationDashboardAsync(userId);

                return Ok(new
                {
                    dashboard,
                    message = dashboard.PendingKDoms.Any()
                        ? $"You have {dashboard.Stats.TotalPending} K-Dom(s) waiting for moderation"
                        : "No K-Doms pending moderation",
                    requiresAttention = dashboard.Stats.TotalPending > 0,
                    urgentItems = dashboard.PendingKDoms.Count(k => k.Priority == ModerationPriority.Urgent)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("stats")]
        public async Task<IActionResult> GetModerationStats()
        {
            try
            {
                var stats = await _moderationService.GetModerationStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("recent-actions")]
        public async Task<IActionResult> GetRecentActions([FromQuery] int limit = 20)
        {
            try
            {
                var actions = await _moderationService.GetRecentModerationActionsAsync(limit);
                return Ok(new
                {
                    actions,
                    totalCount = actions.Count,
                    message = actions.Any() ? "Recent moderation actions retrieved" : "No recent moderation actions found"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("top-moderators")]
        public async Task<IActionResult> GetTopModerators([FromQuery] int days = 30, [FromQuery] int limit = 10)
        {
            try
            {
                var moderators = await _moderationService.GetTopModeratorsAsync(days, limit);
                return Ok(new
                {
                    period = $"Last {days} days",
                    moderators,
                    totalModerators = moderators.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpPost("bulk-action")]
        public async Task<IActionResult> BulkModerate([FromBody] BulkModerationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validare suplimentară pentru reject
            if (dto.Action.ToLower() == "reject" && string.IsNullOrWhiteSpace(dto.Reason))
            {
                return BadRequest(new { error = "Reason is required for rejection." });
            }

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _moderationService.BulkModerateAsync(dto, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
       
        [HttpPost("reject-and-delete/{kdomId}")]
        public async Task<IActionResult> RejectAndDelete(string kdomId, [FromBody] RejectAndDeleteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _moderationService.RejectAndDeleteKDomAsync(kdomId, dto.Reason, userId);

                return Ok(new
                {
                    message = "K-Dom has been rejected and deleted successfully.",
                    action = "rejected_and_deleted",
                    kdomId = kdomId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }



        [HttpGet("priority/{kdomId}")]
        public async Task<IActionResult> GetKDomPriority(string kdomId)
        {
            try
            {
                var priority = await _moderationService.CalculateKDomPriorityAsync(kdomId);
                return Ok(new
                {
                    kdomId,
                    priority = priority.ToString(),
                    priorityLevel = (int)priority,
                    message = GetPriorityMessage(priority)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("can-view-status/{kdomId}")]
        public async Task<IActionResult> CanViewStatus(string kdomId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var canView = await _moderationService.CanUserViewKDomStatusAsync(kdomId, userId);

                return Ok(new
                {
                    kdomId,
                    canView,
                    message = canView ? "User can view K-Dom status" : "Access denied to K-Dom status"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }



        private static string GetPriorityMessage(ModerationPriority priority)
        {
            return priority switch
            {
                ModerationPriority.Urgent => "This K-Dom has been waiting for more than 7 days and needs immediate attention",
                ModerationPriority.High => "This K-Dom has been waiting for more than 3 days",
                ModerationPriority.Normal => "This K-Dom has been waiting for more than 1 day",
                ModerationPriority.Low => "This K-Dom was recently submitted",
                _ => "Unknown priority level"
            };
        }

    }

}
