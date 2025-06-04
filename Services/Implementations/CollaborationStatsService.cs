using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Collaboration;
using KDomBackend.Repositories.Implementations;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class CollaborationStatsService : ICollaborationStatsService
    {
        private readonly IKDomRepository _kdomRepository;
        private readonly IUserService _userService;
        private readonly IKDomEditRepository _kdomEditRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public CollaborationStatsService(
            IKDomRepository kdomRepository,
            IUserService userService,
            IKDomEditRepository kdomEditRepository,
            IAuditLogRepository auditLogRepository)
        {
            _kdomRepository = kdomRepository;
            _userService = userService;
            _kdomEditRepository = kdomEditRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<KDomCollaborationStatsDto> GetKDomCollaborationStatsAsync(string kdomId, int requesterId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            // Only owner, collaborators, and admins can view stats
            var isOwner = kdom.UserId == requesterId;
            var isCollaborator = kdom.Collaborators.Contains(requesterId);
            var user = await _userService.GetUserByIdAsync(requesterId);
            var isAdminOrMod = user?.Role == "admin" || user?.Role == "moderator";

            if (!isOwner && !isCollaborator && !isAdminOrMod)
                throw new UnauthorizedAccessException("Access denied to collaboration statistics.");

            // Get all edit history for this K-Dom
            var allEdits = await _kdomRepository.GetEditsByKDomIdAsync(kdomId);

            // Calculate stats
            var totalCollaborators = kdom.Collaborators.Count;
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentEdits = allEdits.Where(e => e.EditedAt >= thirtyDaysAgo).ToList();
            var activeCollaborators = recentEdits.Select(e => e.UserId).Distinct()
                .Count(userId => kdom.Collaborators.Contains(userId));

            var lastActivity = allEdits.Any() ? allEdits.Max(e => e.EditedAt) : (DateTime?)null;

            // Top collaborators
            var collaboratorEditCounts = allEdits
                .Where(e => kdom.Collaborators.Contains(e.UserId))
                .GroupBy(e => e.UserId)
                .ToDictionary(g => g.Key, g => new { Count = g.Count(), LastEdit = g.Max(e => e.EditedAt) });

            var topCollaborators = new List<CollaboratorEditStatsDto>();
            var totalCollaboratorEdits = collaboratorEditCounts.Values.Sum(v => v.Count);

            foreach (var kvp in collaboratorEditCounts.OrderByDescending(x => x.Value.Count).Take(5))
            {
                var username = await _userService.GetUsernameByUserIdAsync(kvp.Key);
                var editCount = kvp.Value.Count;
                var percentage = totalCollaboratorEdits > 0 ? (double)editCount / totalCollaboratorEdits * 100 : 0;

                topCollaborators.Add(new CollaboratorEditStatsDto
                {
                    UserId = kvp.Key,
                    Username = username,
                    EditCount = editCount,
                    LastEdit = kvp.Value.LastEdit,
                    ContributionPercentage = Math.Round(percentage, 1)
                });
            }

            // Edit distribution
            var ownerEdits = allEdits.Count(e => e.UserId == kdom.UserId);
            var collaboratorEdits = allEdits.Count(e => kdom.Collaborators.Contains(e.UserId));
            var totalEdits = ownerEdits + collaboratorEdits;

            var distribution = new CollaborationDistributionDto
            {
                OwnerEdits = ownerEdits,
                CollaboratorEdits = collaboratorEdits,
                OwnerPercentage = totalEdits > 0 ? Math.Round((double)ownerEdits / totalEdits * 100, 1) : 0,
                CollaboratorPercentage = totalEdits > 0 ? Math.Round((double)collaboratorEdits / totalEdits * 100, 1) : 0
            };

            return new KDomCollaborationStatsDto
            {
                TotalCollaborators = totalCollaborators,
                ActiveCollaborators = activeCollaborators,
                LastCollaboratorActivity = lastActivity,
                TopCollaborators = topCollaborators,
                EditDistribution = distribution
            };
        }

        public async Task<List<CollaboratorReadDto>> GetCollaboratorsAsync(string kdomId, int requesterId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            if (kdom.UserId != requesterId)
                throw new UnauthorizedAccessException("Only the owner can view detailed collaborator information.");

            var result = new List<CollaboratorReadDto>();

            // Obține toate editările pentru acest K-Dom
            var allEdits = await _kdomRepository.GetEditsByKDomIdAsync(kdomId);

            foreach (var userId in kdom.Collaborators)
            {
                var username = await _userService.GetUsernameByUserIdAsync(userId);

                // Filtrează editările pentru acest user
                var userEdits = allEdits.Where(e => e.UserId == userId).OrderByDescending(e => e.EditedAt).ToList();

                result.Add(new CollaboratorReadDto
                {
                    UserId = userId,
                    Username = username ?? "unknown",
                    AddedAt = userEdits.Any() ? userEdits.Last().EditedAt : DateTime.UtcNow.AddDays(-7),
                    EditCount = userEdits.Count,
                    LastActivity = userEdits.FirstOrDefault()?.EditedAt
                });
            }

            return result.OrderByDescending(c => c.EditCount).ToList();
        }

        private async Task<DateTime> GetCollaboratorAddedDateAsync(string kdomId, int userId)
        {
            try
            {
                
                var approvalLog = await _auditLogRepository.GetByKDomAndUserAsync(kdomId, userId, AuditAction.ApproveCollaboration);
                if (approvalLog != null)
                {
                    return approvalLog.CreatedAt;
                }

               
                var firstEdit = await _kdomRepository.GetFirstEditByUserAsync(kdomId, userId);
                if (firstEdit != null)
                {
                    return firstEdit.EditedAt;
                }

                
                return DateTime.UtcNow.AddDays(-7); 
            }
            catch
            {
                return DateTime.UtcNow.AddDays(-7);
            }
        }
    }
}