using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Moderation;
using KDomBackend.Models.DTOs.Notification;
using KDomBackend.Models.Entities;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class ModerationService : IModerationService
    {
        private readonly IKDomRepository _kdomRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public ModerationService(
            IKDomRepository kdomRepository,
            IAuditLogRepository auditLogRepository,
            IUserService userService,
            INotificationService notificationService)
        {
            _kdomRepository = kdomRepository;
            _auditLogRepository = auditLogRepository;
            _userService = userService;
            _notificationService = notificationService;
        }

        public async Task<ModerationDashboardDto> GetModerationDashboardAsync(int moderatorId)
        {
            var stats = await GetModerationStatsAsync();
            var pendingKDoms = await GetPendingKDomsForModerationAsync();
            var recentActions = await GetRecentModerationActionsAsync(10);

            return new ModerationDashboardDto
            {
                Stats = stats,
                PendingKDoms = pendingKDoms,
                RecentActions = recentActions
            };
        }

        public async Task<BulkModerationResultDto> BulkModerateAsync(BulkModerationDto dto, int moderatorId)
        {
            var results = new List<ModerationResultItemDto>();
            var successCount = 0;

            foreach (var kdomId in dto.KDomIds)
            {
                try
                {
                    var kdom = await _kdomRepository.GetByIdAsync(kdomId);
                    if (kdom == null)
                    {
                        results.Add(new ModerationResultItemDto
                        {
                            KDomId = kdomId,
                            KDomTitle = "Unknown",
                            Success = false,
                            Error = "K-Dom not found"
                        });
                        continue;
                    }

                    if (dto.Action.ToLower() == "approve")
                    {
                        await ApproveKDomAsync(kdomId, moderatorId);
                        results.Add(new ModerationResultItemDto
                        {
                            KDomId = kdomId,
                            KDomTitle = kdom.Title,
                            Success = true,
                            Message = "Approved successfully"
                        });
                    }
                    else if (dto.Action.ToLower() == "reject")
                    {
                        if (dto.DeleteRejected)
                        {
                            await RejectAndDeleteKDomAsync(kdomId, dto.Reason ?? "Bulk rejection", moderatorId);
                            results.Add(new ModerationResultItemDto
                            {
                                KDomId = kdomId,
                                KDomTitle = kdom.Title,
                                Success = true,
                                Message = "Rejected and deleted successfully"
                            });
                        }
                        else
                        {
                            await RejectKDomAsync(kdomId, dto.Reason ?? "Bulk rejection", moderatorId);
                            results.Add(new ModerationResultItemDto
                            {
                                KDomId = kdomId,
                                KDomTitle = kdom.Title,
                                Success = true,
                                Message = "Rejected successfully"
                            });
                        }
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    results.Add(new ModerationResultItemDto
                    {
                        KDomId = kdomId,
                        KDomTitle = "Unknown",
                        Success = false,
                        Error = ex.Message
                    });
                }
            }

            var failureCount = dto.KDomIds.Count - successCount;

            return new BulkModerationResultDto
            {
                Message = $"Bulk moderation completed. {successCount} succeeded, {failureCount} failed.",
                SuccessCount = successCount,
                FailureCount = failureCount,
                TotalProcessed = dto.KDomIds.Count,
                Results = results
            };
        }

        public async Task RejectAndDeleteKDomAsync(string kdomId, string reason, int moderatorId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            // Marcăm ca respins
            await _kdomRepository.RejectAsync(kdomId, reason);

            // Audit log pentru respingere
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = moderatorId,
                Action = AuditAction.RejectKDom,
                TargetType = AuditTargetType.KDom,
                TargetId = kdomId,
                CreatedAt = DateTime.UtcNow,
                Details = $"Rejected and deleted: {reason}"
            });

            // Notificare către autor
            await _notificationService.CreateNotificationAsync(new NotificationCreateDto
            {
                UserId = kdom.UserId,
                Type = NotificationType.KDomRejected,
                Message = $"Your K-Dom '{kdom.Title}' has been rejected and removed. Reason: {reason}",
                TriggeredByUserId = moderatorId,
                TargetType = ContentType.KDom,
                TargetId = kdomId
            });

            // Ștergem K-Dom-ul
            await _kdomRepository.DeleteAsync(kdomId);

            // Audit log pentru ștergere
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = moderatorId,
                Action = AuditAction.DeleteKDom,
                TargetType = AuditTargetType.KDom,
                TargetId = kdomId,
                CreatedAt = DateTime.UtcNow,
                Details = $"K-Dom deleted after rejection: {kdom.Title}"
            });
        }

        public async Task<ModerationHistoryDto> GetUserModerationHistoryAsync(int userId)
        {
            var userKDoms = await _kdomRepository.GetUserKDomsWithStatusAsync(userId);
            var userSubmissions = new List<UserKDomStatusDto>();

            foreach (var kdom in userKDoms)
            {
                var status = await ConvertToUserKDomStatusDto(kdom);
                userSubmissions.Add(status);
            }

            var userStats = await CalculateUserModerationStatsAsync(userId, userKDoms);
            var recentDecisions = await GetUserRecentDecisionsAsync(userId, 10);

            return new ModerationHistoryDto
            {
                AllSubmissions = userSubmissions,
                UserStats = userStats,
                RecentDecisions = recentDecisions
            };
        }

        public async Task<List<UserKDomStatusDto>> GetUserKDomStatusesAsync(int userId)
        {
            var userKDoms = await _kdomRepository.GetUserKDomsWithStatusAsync(userId);
            var statuses = new List<UserKDomStatusDto>();

            foreach (var kdom in userKDoms)
            {
                var status = await ConvertToUserKDomStatusDto(kdom);
                statuses.Add(status);
            }

            return statuses.OrderByDescending(s => s.CreatedAt).ToList();
        }

        public async Task<UserKDomStatusDto> GetKDomStatusAsync(string kdomId, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            if (kdom.UserId != userId)
                throw new UnauthorizedAccessException("You can only view status of your own K-Doms.");

            return await ConvertToUserKDomStatusDto(kdom);
        }

        public async Task<ModerationStatsDto> GetModerationStatsAsync()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var stats = new ModerationStatsDto
            {
                TotalPending = await _kdomRepository.GetPendingCountAsync(),
                TotalApprovedToday = await _kdomRepository.GetApprovedCountAsync(today),
                TotalRejectedToday = await _kdomRepository.GetRejectedCountAsync(today),
                TotalApprovedThisWeek = await _kdomRepository.GetApprovedCountAsync(weekStart),
                TotalRejectedThisWeek = await _kdomRepository.GetRejectedCountAsync(weekStart),
                TotalApprovedThisMonth = await _kdomRepository.GetApprovedCountAsync(monthStart),
                TotalRejectedThisMonth = await _kdomRepository.GetRejectedCountAsync(monthStart),
                AverageProcessingTimeHours = (await _kdomRepository.GetAverageProcessingTimeAsync()).TotalHours,
                ModeratorActivity = await GetModeratorActivityAsync(30)
            };

            return stats;
        }

        public async Task<List<ModerationActionDto>> GetRecentModerationActionsAsync(int limit = 20)
        {
            var actions = await _auditLogRepository.GetModerationActionsAsync(limit);
            var result = new List<ModerationActionDto>();

            foreach (var action in actions)
            {
                var kdom = await _kdomRepository.GetByIdAsync(action.TargetId);
                var moderatorUsername = await _userService.GetUsernameByUserIdAsync(action.UserId ?? 0);
                var authorUsername = kdom != null ? await _userService.GetUsernameByUserIdAsync(kdom.UserId) : "Unknown";

                var processingTime = TimeSpan.Zero;
                if (kdom != null)
                {
                    processingTime = action.CreatedAt - kdom.CreatedAt;
                }

                result.Add(new ModerationActionDto
                {
                    Id = action.Id,
                    KDomId = action.TargetId,
                    KDomTitle = kdom?.Title ?? "Deleted K-Dom",
                    ModeratorUsername = moderatorUsername,
                    Decision = action.Action == AuditAction.ApproveKDom ? ModerationDecision.Approved : ModerationDecision.Rejected,
                    Reason = action.Details,
                    ActionDate = action.CreatedAt,
                    ProcessingTime = processingTime,
                    AuthorUsername = authorUsername
                });
            }

            return result;
        }

        public async Task<List<ModeratorActivityDto>> GetTopModeratorsAsync(int days = 30, int limit = 10)
        {
            return await GetModeratorActivityAsync(days, limit);
        }

        public async Task<bool> CanUserViewKDomStatusAsync(string kdomId, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null) return false;

            // Utilizatorul poate vedea statusul propriilor K-Dom-uri
            if (kdom.UserId == userId) return true;

            // Admins și moderatorii pot vedea toate statusurile
            var user = await _userService.GetUserByIdAsync(userId);
            return user?.Role == "admin" || user?.Role == "moderator";
        }

        public async Task<ModerationPriority> CalculateKDomPriorityAsync(string kdomId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null) return ModerationPriority.Normal;

            var waitingTime = DateTime.UtcNow - kdom.CreatedAt;

            // Prioritate bazată pe timpul de așteptare
            if (waitingTime.TotalDays > 7)
                return ModerationPriority.Urgent;
            if (waitingTime.TotalDays > 3)
                return ModerationPriority.High;
            if (waitingTime.TotalDays > 1)
                return ModerationPriority.Normal;

            return ModerationPriority.Low;
        }

        // ✅ METODELE MUTATE DIN KDomFlowService

        public async Task ApproveKDomAsync(string kdomId, int moderatorId)
        {
            await _kdomRepository.ApproveAsync(kdomId);

            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = moderatorId,
                Action = AuditAction.ApproveKDom,
                TargetType = AuditTargetType.KDom,
                TargetId = kdomId,
                CreatedAt = DateTime.UtcNow
            });

            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom != null)
            {
                await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = kdom.UserId,
                    Type = NotificationType.KDomApproved,
                    Message = $"Your K-Dom '{kdom.Title}' has been approved.",
                    TriggeredByUserId = moderatorId,
                    TargetType = ContentType.KDom,
                    TargetId = kdomId
                });
            }
        }

        public async Task RejectKDomAsync(string kdomId, string reason, int moderatorId)
        {
            await _kdomRepository.RejectAsync(kdomId, reason);

            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = moderatorId,
                Action = AuditAction.RejectKDom,
                TargetType = AuditTargetType.KDom,
                TargetId = kdomId,
                CreatedAt = DateTime.UtcNow,
                Details = reason
            });

            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom != null)
            {
                await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = kdom.UserId,
                    Type = NotificationType.KDomRejected,
                    Message = $"Your K-Dom '{kdom.Title}' has been rejected. Reason: {reason}",
                    TriggeredByUserId = moderatorId,
                    TargetType = ContentType.KDom,
                    TargetId = kdomId
                });
            }
        }

        public async Task ForceDeleteKDomAsync(string kdomId, int requesterId, string reason)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            // Verifică permisiunile - doar admins pot șterge forțat
            var user = await _userService.GetUserByIdAsync(requesterId);
            if (user?.Role != "admin")
                throw new UnauthorizedAccessException("Only administrators can force delete K-Doms.");

            // Notificare către autor înainte de ștergere
            await _notificationService.CreateNotificationAsync(new NotificationCreateDto
            {
                UserId = kdom.UserId,
                Type = NotificationType.SystemMessage,
                Message = $"Your K-Dom '{kdom.Title}' has been removed by an administrator. Reason: {reason}",
                TriggeredByUserId = requesterId,
                TargetType = ContentType.KDom,
                TargetId = kdomId
            });

            // Audit log înainte de ștergere
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = requesterId,
                Action = AuditAction.ForceDeleteKDom,
                TargetType = AuditTargetType.KDom,
                TargetId = kdomId,
                CreatedAt = DateTime.UtcNow,
                Details = $"Force deleted: {reason}"
            });

            // Ștergere K-Dom
            await _kdomRepository.DeleteAsync(kdomId);
        }

        // Metode helper private

        private async Task<List<KDomModerationDto>> GetPendingKDomsForModerationAsync()
        {
            var pendingKDoms = await _kdomRepository.GetPendingKdomsAsync();
            var result = new List<KDomModerationDto>();

            foreach (var kdom in pendingKDoms)
            {
                var authorUsername = await _userService.GetUsernameByUserIdAsync(kdom.UserId);
                var priority = await CalculateKDomPriorityAsync(kdom.Id);

                string? parentTitle = null;
                if (!string.IsNullOrEmpty(kdom.ParentId))
                {
                    var parent = await _kdomRepository.GetByIdAsync(kdom.ParentId);
                    parentTitle = parent?.Title;
                }

                result.Add(new KDomModerationDto
                {
                    Id = kdom.Id,
                    Title = kdom.Title,
                    Slug = kdom.Slug,
                    Description = kdom.Description,
                    ContentHtml = kdom.ContentHtml,
                    Hub = kdom.Hub,
                    Language = kdom.Language,
                    IsForKids = kdom.IsForKids,
                    Theme = kdom.Theme,
                    AuthorUsername = authorUsername,
                    AuthorId = kdom.UserId,
                    CreatedAt = kdom.CreatedAt,
                    ParentId = kdom.ParentId,
                    ParentTitle = parentTitle,
                    Priority = priority
                });
            }

            // Sortează după prioritate și apoi după data creării
            return result.OrderByDescending(k => k.Priority)
                        .ThenBy(k => k.CreatedAt)
                        .ToList();
        }

        private async Task<List<ModeratorActivityDto>> GetModeratorActivityAsync(int days, int limit = 10)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            // ✅ Folosim doar AuditLogRepository pentru activitatea moderatorilor
            var moderatorStats = await _auditLogRepository.GetModeratorStatsAsync(fromDate);
            var result = new List<ModeratorActivityDto>();

            foreach (var kvp in moderatorStats.OrderByDescending(x => x.Value).Take(limit))
            {
                var moderatorUsername = await _userService.GetUsernameByUserIdAsync(kvp.Key);
                var actions = await _auditLogRepository.GetModerationActionsByModeratorAsync(kvp.Key, fromDate);

                var approvedCount = actions.Count(a => a.Action == AuditAction.ApproveKDom);
                var rejectedCount = actions.Count(a => a.Action == AuditAction.RejectKDom);

                result.Add(new ModeratorActivityDto
                {
                    ModeratorUsername = moderatorUsername,
                    ApprovedCount = approvedCount,
                    RejectedCount = rejectedCount,
                    TotalActions = kvp.Value
                });
            }

            return result;
        }

        private async Task<UserKDomStatusDto> ConvertToUserKDomStatusDto(KDom kdom)
        {
            var status = KDomModerationStatus.Pending;
            if (kdom.IsApproved) status = KDomModerationStatus.Approved;
            else if (kdom.IsRejected) status = KDomModerationStatus.Rejected;

            var moderationAction = await _auditLogRepository.GetLastModerationActionAsync(kdom.Id);
            var moderatorUsername = moderationAction?.UserId != null
                ? await _userService.GetUsernameByUserIdAsync(moderationAction.UserId.Value)
                : null;

            TimeSpan? processingTime = null;
            DateTime? moderatedAt = null;

            if (moderationAction != null)
            {
                moderatedAt = moderationAction.CreatedAt;
                processingTime = moderationAction.CreatedAt - kdom.CreatedAt;
            }

            return new UserKDomStatusDto
            {
                Id = kdom.Id,
                Title = kdom.Title,
                Slug = kdom.Slug,
                CreatedAt = kdom.CreatedAt,
                Status = status,
                RejectionReason = kdom.RejectionReason,
                ModeratedAt = moderatedAt,
                ModeratorUsername = moderatorUsername,
                ProcessingTime = processingTime,
                CanEdit = status == KDomModerationStatus.Approved,
                CanResubmit = false // Pentru viitor - când implementăm resubmiterea
            };
        }

        private async Task<ModerationStatsDto> CalculateUserModerationStatsAsync(int userId, List<KDom> userKDoms)
        {
            var totalSubmitted = userKDoms.Count;
            var totalApproved = userKDoms.Count(k => k.IsApproved);
            var totalRejected = userKDoms.Count(k => k.IsRejected);
            var totalPending = userKDoms.Count(k => !k.IsApproved && !k.IsRejected);

            var approvalRate = totalSubmitted > 0 ? (double)totalApproved / totalSubmitted * 100 : 0;

            var processedKDoms = userKDoms.Where(k => k.IsApproved || k.IsRejected).ToList();
            var averageProcessingTime = TimeSpan.Zero;

            if (processedKDoms.Any())
            {
                var totalProcessingTime = TimeSpan.Zero;
                var processedCount = 0;

                foreach (var kdom in processedKDoms)
                {
                    var moderationAction = await _auditLogRepository.GetLastModerationActionAsync(kdom.Id);
                    if (moderationAction != null)
                    {
                        totalProcessingTime = totalProcessingTime.Add(moderationAction.CreatedAt - kdom.CreatedAt);
                        processedCount++;
                    }
                }

                if (processedCount > 0)
                {
                    averageProcessingTime = TimeSpan.FromTicks(totalProcessingTime.Ticks / processedCount);
                }
            }

            return new ModerationStatsDto
            {
                TotalPending = totalPending,
                TotalApprovedToday = totalApproved, // Simplificat pentru demo
                TotalRejectedToday = totalRejected, // Simplificat pentru demo
                AverageProcessingTimeHours = averageProcessingTime.TotalHours
            };
        }

        private async Task<List<ModerationActionDto>> GetUserRecentDecisionsAsync(int userId, int limit)
        {
            var userKDoms = await _kdomRepository.GetUserKDomsWithStatusAsync(userId);
            var kdomIds = userKDoms.Where(k => k.IsApproved || k.IsRejected)
                                  .Select(k => k.Id)
                                  .ToList();

            var recentActions = new List<ModerationActionDto>();

            foreach (var kdomId in kdomIds.Take(limit))
            {
                var action = await _auditLogRepository.GetLastModerationActionAsync(kdomId);
                if (action != null)
                {
                    var kdom = userKDoms.First(k => k.Id == kdomId);
                    var moderatorUsername = await _userService.GetUsernameByUserIdAsync(action.UserId ?? 0);

                    recentActions.Add(new ModerationActionDto
                    {
                        Id = action.Id,
                        KDomId = kdomId,
                        KDomTitle = kdom.Title,
                        ModeratorUsername = moderatorUsername,
                        Decision = action.Action == AuditAction.ApproveKDom ? ModerationDecision.Approved : ModerationDecision.Rejected,
                        Reason = action.Details,
                        ActionDate = action.CreatedAt,
                        ProcessingTime = action.CreatedAt - kdom.CreatedAt,
                        AuthorUsername = await _userService.GetUsernameByUserIdAsync(userId)
                    });
                }
            }

            return recentActions.OrderByDescending(a => a.ActionDate).ToList();
        }
    }
}