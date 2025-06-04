// Controllers/CollaborationController.cs - DEDICATED CONTROLLER for all collaboration features

using KDomBackend.Models.DTOs.Collaboration;
using KDomBackend.Models.Entities;
using KDomBackend.Services.Implementations;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/collaboration")]
    public class CollaborationController : ControllerBase
    {
        private readonly ICollaborationRequestService _collaborationRequestService;
        private readonly ICollaborationStatsService _collaborationStatsService;

        public CollaborationController(
            ICollaborationRequestService collaborationRequestService,
            ICollaborationStatsService collaborationStatsService)
        {
            _collaborationRequestService = collaborationRequestService;
            _collaborationStatsService = collaborationStatsService;
        }

        #region Collaboration Requests Management

        [Authorize]
        [HttpGet("requests/sent")]
        public async Task<IActionResult> GetSentRequests()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var requests = await _collaborationRequestService.GetSentRequestsAsync(userId);

                var pendingCount = requests.Count(r => r.Status == Enums.CollaborationRequestStatus.Pending);
                var approvedCount = requests.Count(r => r.Status == Enums.CollaborationRequestStatus.Approved);
                var rejectedCount = requests.Count(r => r.Status == Enums.CollaborationRequestStatus.Rejected);

                return Ok(new
                {
                    requests,
                    summary = new
                    {
                        total = requests.Count,
                        pending = pendingCount,
                        approved = approvedCount,
                        rejected = rejectedCount
                    },
                    message = requests.Any() ?
                        $"Found {requests.Count} collaboration requests you've sent" :
                        "You haven't sent any collaboration requests yet"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet("requests/received")]
        public async Task<IActionResult> GetReceivedRequests()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var requests = await _collaborationRequestService.GetReceivedRequestsAsync(userId);

                var pendingCount = requests.Count(r => r.Status == Enums.CollaborationRequestStatus.Pending);
                var approvedCount = requests.Count(r => r.Status == Enums.CollaborationRequestStatus.Approved);
                var rejectedCount = requests.Count(r => r.Status == Enums.CollaborationRequestStatus.Rejected);

                // Group by K-Dom for better organization
                var groupedByKDom = requests
                    .GroupBy(r => r.KDomTitle)
                    .Select(g => new
                    {
                        kdomTitle = g.Key,
                        requests = g.OrderByDescending(r => r.CreatedAt).ToList(),
                        pendingCount = g.Count(r => r.Status == Enums.CollaborationRequestStatus.Pending)
                    })
                    .OrderByDescending(g => g.pendingCount)
                    .ThenBy(g => g.kdomTitle)
                    .ToList();

                return Ok(new
                {
                    requests,
                    groupedByKDom,
                    summary = new
                    {
                        total = requests.Count,
                        pending = pendingCount,
                        approved = approvedCount,
                        rejected = rejectedCount,
                        kdomsWithRequests = groupedByKDom.Count
                    },
                    message = requests.Any() ?
                        $"Found {requests.Count} collaboration requests for your K-Doms" :
                        "No collaboration requests received yet"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("requests/all")]
        public async Task<IActionResult> GetAllRequests()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var sentRequests = await _collaborationRequestService.GetSentRequestsAsync(userId);
                var receivedRequests = await _collaborationRequestService.GetReceivedRequestsAsync(userId);

                var pendingSent = sentRequests.Count(r => r.Status == Enums.CollaborationRequestStatus.Pending);
                var pendingReceived = receivedRequests.Count(r => r.Status == Enums.CollaborationRequestStatus.Pending);

                return Ok(new
                {
                    sent = new
                    {
                        requests = sentRequests,
                        total = sentRequests.Count,
                        pending = pendingSent
                    },
                    received = new
                    {
                        requests = receivedRequests,
                        total = receivedRequests.Count,
                        pending = pendingReceived
                    },
                    summary = new
                    {
                        totalSent = sentRequests.Count,
                        totalReceived = receivedRequests.Count,
                        pendingSent,
                        pendingReceived,
                        totalPending = pendingSent + pendingReceived
                    },
                    needsAttention = pendingReceived > 0,
                    message = pendingReceived > 0 ?
                        $"You have {pendingReceived} pending collaboration request(s) to review" :
                        "No pending requests need your attention"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("stats/quick")]
        public async Task<IActionResult> GetQuickStats()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var sentRequests = await _collaborationRequestService.GetSentRequestsAsync(userId);
                var receivedRequests = await _collaborationRequestService.GetReceivedRequestsAsync(userId);

                var stats = new
                {
                    sentRequests = new
                    {
                        total = sentRequests.Count,
                        pending = sentRequests.Count(r => r.Status == Enums.CollaborationRequestStatus.Pending),
                        approved = sentRequests.Count(r => r.Status == Enums.CollaborationRequestStatus.Approved),
                        rejected = sentRequests.Count(r => r.Status == Enums.CollaborationRequestStatus.Rejected)
                    },
                    receivedRequests = new
                    {
                        total = receivedRequests.Count,
                        pending = receivedRequests.Count(r => r.Status == Enums.CollaborationRequestStatus.Pending),
                        approved = receivedRequests.Count(r => r.Status == Enums.CollaborationRequestStatus.Approved),
                        rejected = receivedRequests.Count(r => r.Status == Enums.CollaborationRequestStatus.Rejected)
                    },
                    hasNotifications = receivedRequests.Any(r => r.Status == Enums.CollaborationRequestStatus.Pending)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [Authorize]
        [HttpPost("requests/bulk-action")]
        public async Task<IActionResult> BulkActionRequests([FromBody] BulkRequestActionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var results = new List<BulkActionResultDto>();

                foreach (var requestId in dto.RequestIds)
                {
                    try
                    {
                        if (dto.Action.ToLower() == "approve")
                        {
                            await _collaborationRequestService.ApproveAsync(dto.KDomId, requestId, userId);
                        }
                        else if (dto.Action.ToLower() == "reject")
                        {
                            await _collaborationRequestService.RejectAsync(dto.KDomId, requestId, userId, dto.Reason);
                        }
                        else
                        {
                            throw new ArgumentException("Invalid action. Use 'approve' or 'reject'.");
                        }

                        results.Add(new BulkActionResultDto
                        {
                            RequestId = requestId,
                            Success = true,
                            Message = $"Request {dto.Action}d successfully"
                        });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new BulkActionResultDto
                        {
                            RequestId = requestId,
                            Success = false,
                            Message = ex.Message
                        });
                    }
                }

                var successCount = results.Count(r => r.Success);
                var failureCount = results.Count(r => !r.Success);

                return Ok(new
                {
                    message = $"Bulk operation completed. {successCount} succeeded, {failureCount} failed.",
                    successCount,
                    failureCount,
                    totalProcessed = results.Count,
                    results
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region K-Dom Collaboration Stats & Management


        [Authorize]
        [HttpGet("kdoms/{kdomId}/stats")]
        public async Task<IActionResult> GetKDomCollaborationStats(string kdomId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var stats = await _collaborationStatsService.GetKDomCollaborationStatsAsync(kdomId, userId);

                return Ok(new
                {
                    kdomId,
                    stats,
                    message = "Collaboration statistics retrieved successfully"
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


        [Authorize]
        [HttpGet("kdoms/{kdomId}/collaborators")]
        public async Task<IActionResult> GetKDomCollaborators(string kdomId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var collaborators = await _collaborationStatsService.GetCollaboratorsAsync(kdomId, userId);

                return Ok(new
                {
                    kdomId,
                    collaborators,
                    totalCount = collaborators.Count,
                    activeCount = collaborators.Count(c => c.LastActivity?.AddDays(30) > DateTime.UtcNow),
                    message = "Collaborators retrieved successfully"
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

        [Authorize]
        [HttpGet("{id}/collaborators")]
        public async Task<IActionResult> GetCollaborators(string id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var collaborators = await _collaborationStatsService.GetCollaboratorsAsync(id, userId);
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

        #endregion
    }
}