using KDomBackend.Models.DTOs.Moderation;
using KDomBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KDomBackend.Controllers
{
    [ApiController]
    [Route("api/user/moderation")]
    [Authorize]
    public class UserModerationController : ControllerBase
    {
        private readonly IModerationService _moderationService;

        public UserModerationController(IModerationService moderationService)
        {
            _moderationService = moderationService;
        }

        /// <summary>
        /// Obține istoricul complet de moderare pentru utilizatorul curent
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetMyModerationHistory()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var history = await _moderationService.GetUserModerationHistoryAsync(userId);

                return Ok(new
                {
                    history,
                    summary = new
                    {
                        totalSubmitted = history.AllSubmissions.Count,
                        pending = history.AllSubmissions.Count(s => s.Status == KDomModerationStatus.Pending),
                        approved = history.AllSubmissions.Count(s => s.Status == KDomModerationStatus.Approved),
                        rejected = history.AllSubmissions.Count(s => s.Status == KDomModerationStatus.Rejected),
                        approvalRate = history.AllSubmissions.Count > 0
                            ? Math.Round((double)history.AllSubmissions.Count(s => s.Status == KDomModerationStatus.Approved) / history.AllSubmissions.Count * 100, 1)
                            : 0
                    },
                    message = history.AllSubmissions.Any()
                        ? "Your K-Dom submission history retrieved successfully"
                        : "You haven't submitted any K-Doms yet"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține statusurile tuturor K-Dom-urilor utilizatorului curent
        /// </summary>
        [HttpGet("my-kdoms-status")]
        public async Task<IActionResult> GetMyKDomsStatus()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var statuses = await _moderationService.GetUserKDomStatusesAsync(userId);

                var groupedByStatus = statuses.GroupBy(s => s.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.ToList());

                return Ok(new
                {
                    statuses,
                    groupedByStatus,
                    summary = new
                    {
                        total = statuses.Count,
                        pending = statuses.Count(s => s.Status == KDomModerationStatus.Pending),
                        approved = statuses.Count(s => s.Status == KDomModerationStatus.Approved),
                        rejected = statuses.Count(s => s.Status == KDomModerationStatus.Rejected),
                        canEdit = statuses.Count(s => s.CanEdit),
                        canResubmit = statuses.Count(s => s.CanResubmit)
                    },
                    notifications = new
                    {
                        hasPendingItems = statuses.Any(s => s.Status == KDomModerationStatus.Pending),
                        hasRejectedItems = statuses.Any(s => s.Status == KDomModerationStatus.Rejected),
                        pendingCount = statuses.Count(s => s.Status == KDomModerationStatus.Pending)
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține statusul unui K-Dom specific
        /// </summary>
        [HttpGet("kdom-status/{kdomId}")]
        public async Task<IActionResult> GetKDomStatus(string kdomId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var status = await _moderationService.GetKDomStatusAsync(kdomId, userId);

                return Ok(new
                {
                    status,
                    timeline = new
                    {
                        submitted = status.CreatedAt,
                        moderated = status.ModeratedAt,
                        processingTime = status.ProcessingTime,
                        isProcessed = status.ModeratedAt.HasValue
                    },
                    actions = new
                    {
                        canEdit = status.CanEdit,
                        canResubmit = status.CanResubmit,
                        isWaitingForModeration = status.Status == KDomModerationStatus.Pending
                    },
                    message = GetStatusMessage(status.Status)
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

        /// <summary>
        /// Obține doar K-Dom-urile în așteptare ale utilizatorului
        /// </summary>
        [HttpGet("pending-kdoms")]
        public async Task<IActionResult> GetMyPendingKDoms()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var allStatuses = await _moderationService.GetUserKDomStatusesAsync(userId);
                var pendingKDoms = allStatuses.Where(s => s.Status == KDomModerationStatus.Pending).ToList();

                return Ok(new
                {
                    pendingKDoms,
                    count = pendingKDoms.Count,
                    waitingTimes = pendingKDoms.Select(k => new
                    {
                        kdomId = k.Id,
                        title = k.Title,
                        waitingTime = DateTime.UtcNow - k.CreatedAt,
                        waitingDays = (DateTime.UtcNow - k.CreatedAt).Days
                    }).ToList(),
                    message = pendingKDoms.Any()
                        ? $"You have {pendingKDoms.Count} K-Dom(s) waiting for moderation"
                        : "All your K-Doms have been processed"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține doar K-Dom-urile respinse ale utilizatorului
        /// </summary>
        [HttpGet("rejected-kdoms")]
        public async Task<IActionResult> GetMyRejectedKDoms()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var allStatuses = await _moderationService.GetUserKDomStatusesAsync(userId);
                var rejectedKDoms = allStatuses.Where(s => s.Status == KDomModerationStatus.Rejected).ToList();

                return Ok(new
                {
                    rejectedKDoms,
                    count = rejectedKDoms.Count,
                    rejectionReasons = rejectedKDoms
                        .Where(k => !string.IsNullOrEmpty(k.RejectionReason))
                        .GroupBy(k => k.RejectionReason)
                        .Select(g => new { reason = g.Key, count = g.Count() })
                        .OrderByDescending(x => x.count)
                        .ToList(),
                    message = rejectedKDoms.Any()
                        ? $"You have {rejectedKDoms.Count} rejected K-Dom(s). Review the feedback to improve future submissions."
                        : "None of your K-Doms have been rejected"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obține statistici simple pentru dashboard-ul utilizatorului
        /// </summary>
        [HttpGet("quick-stats")]
        public async Task<IActionResult> GetMyQuickStats()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var statuses = await _moderationService.GetUserKDomStatusesAsync(userId);

                var stats = new
                {
                    totalSubmitted = statuses.Count,
                    pending = statuses.Count(s => s.Status == KDomModerationStatus.Pending),
                    approved = statuses.Count(s => s.Status == KDomModerationStatus.Approved),
                    rejected = statuses.Count(s => s.Status == KDomModerationStatus.Rejected),
                    approvalRate = statuses.Count > 0
                        ? Math.Round((double)statuses.Count(s => s.Status == KDomModerationStatus.Approved) / statuses.Count * 100, 1)
                        : 0,
                    averageProcessingTime = statuses.Where(s => s.ProcessingTime.HasValue).Any()
                        ? statuses.Where(s => s.ProcessingTime.HasValue)
                                 .Average(s => s.ProcessingTime!.Value.TotalHours)
                        : 0,
                    lastSubmission = statuses.Any() ? statuses.Max(s => s.CreatedAt) : (DateTime?)null,
                    hasNotifications = statuses.Any(s => s.Status == KDomModerationStatus.Pending) ||
                                     statuses.Any(s => s.Status == KDomModerationStatus.Rejected)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #region Helper methods

        private static string GetStatusMessage(KDomModerationStatus status)
        {
            return status switch
            {
                KDomModerationStatus.Pending => "Your K-Dom is waiting for moderation. This usually takes 1-3 days.",
                KDomModerationStatus.Approved => "Congratulations! Your K-Dom has been approved and is now live.",
                KDomModerationStatus.Rejected => "Your K-Dom was rejected. Please review the feedback and consider resubmitting with improvements.",
                KDomModerationStatus.Deleted => "This K-Dom has been removed from the platform.",
                _ => "Unknown status"
            };
        }

        #endregion
    }
}