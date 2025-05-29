using KDomBackend.Enums;
using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Models.DTOs.Notification;
using KDomBackend.Models.Entities;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;
using KDomBackend.Services.Validation;
using Newtonsoft.Json;

namespace KDomBackend.Services.Implementations
{
    public class KDomFlowService : IKDomFlowService
    {
        private readonly IKDomRepository _kdomRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly KDomValidator _validator;
        private readonly KDomMetadataValidator _metadataValidator;
        private readonly ILogger<KDomFlowService> _logger;
        

        public KDomFlowService(IKDomRepository kdomRepository,
            IUserService userService,
            IAuditLogRepository auditLogRepository,
            INotificationService notificationService,
            IUserRepository userRepository,
            KDomValidator validator,
            KDomMetadataValidator metadataValidator,
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            IKDomFollowRepository kdomFollowRepository,
            IKDomEditRepository kdomEditRepository,
            ILogger<KDomFlowService> logger
            )
        {
            _kdomRepository = kdomRepository;
            _auditLogRepository = auditLogRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _validator = validator;
            _metadataValidator = metadataValidator;
            _logger = logger;
        }


        public async Task CreateKDomAsync(KDomCreateDto dto, int userId)
        {
            var sanitizedHtml = HtmlSanitizerHelper.Sanitize(dto.ContentHtml);
            await _validator.CheckDuplicateOrSuggestAsync(dto.Title);
            _logger.LogInformation("DTO primit: {dto}", JsonConvert.SerializeObject(dto));


            var kdom = new KDom
            {
                Title = dto.Title,
                Slug = dto.Slug,
                Description = dto.Description,
                Hub = dto.Hub,
                Language = dto.Language,
                IsForKids = dto.IsForKids,
                Theme = dto.Theme,
                ContentHtml = sanitizedHtml,
                UserId = userId,
                ParentId = dto.ParentId, //  nou
                CreatedAt = DateTime.UtcNow
            };
            _logger.LogInformation("KDom entity before insert: {kdom}", JsonConvert.SerializeObject(kdom));

            await _kdomRepository.CreateAsync(kdom);

            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                Action = AuditAction.CreateKDom,
                TargetType = AuditTargetType.KDom,
                TargetId = kdom.Id,
                Details = kdom.Title,
                CreatedAt = DateTime.UtcNow
            });

            var moderators = await _userRepository.GetUsersByRolesAsync(new[] { "admin", "moderator" });

            foreach (var moderator in moderators)
            {
                await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = moderator.Id,
                    Type = NotificationType.KDomPending,
                    Message = $"A new K-Dom is pending.",
                    TriggeredByUserId = userId,
                    TargetType = ContentType.KDom,
                    TargetId = kdom.Id
                });
            }

        }

        public async Task CreateSubKDomAsync(string parentId, KDomSubCreateDto dto, int userId)
        {
            var parent = await _kdomRepository.GetByIdAsync(parentId);
            if (parent == null)
                throw new Exception("Parent not found.");

            if (parent.UserId != userId && !parent.Collaborators.Contains(userId))
                throw new UnauthorizedAccessException("You do not have permission to create a page.");

            var slug = SlugHelper.GenerateSlug(dto.Title);
            var exists = await _kdomRepository.ExistsByTitleOrSlugAsync(dto.Title, slug);
            if (exists)
                throw new Exception("A K-Dom with this title already exists.");

            var sanitizedHtml = HtmlSanitizerHelper.Sanitize(dto.ContentHtml);

            var subKdom = new KDom
            {
                Title = dto.Title,
                Slug = slug,
                Description = dto.Description,
                Hub = parent.Hub,
                Language = parent.Language,
                IsForKids = parent.IsForKids,
                Theme = dto.Theme,
                ContentHtml = sanitizedHtml,
                UserId = parent.UserId,
                ParentId = parent.Id,
                Collaborators = parent.Collaborators.ToList(),
                CreatedAt = DateTime.UtcNow
            };

            await _kdomRepository.CreateAsync(subKdom);

            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                Action = AuditAction.CreateKDom,
                TargetType = AuditTargetType.KDom,
                TargetId = subKdom.Id,
                Details = $"SubK-Dom: {dto.Title} (parent {parent.Title})",
                CreatedAt = DateTime.UtcNow
            });

            var moderators = await _userRepository.GetUsersByRolesAsync(new[] { "admin", "moderator" });

            foreach (var mod in moderators)
            {
                await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = mod.Id,
                    Type = NotificationType.KDomPending,
                    Message = $"A new subK-Dom has been created: {dto.Title}.",
                    TriggeredByUserId = userId,
                    TargetType = ContentType.KDom,
                    TargetId = subKdom.Id
                });
            }
        }

        public async Task<bool> EditKDomAsync(KDomEditDto dto, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(dto.KDomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            var sanitizedHtml = HtmlSanitizerHelper.Sanitize(dto.ContentHtml);

            // nu salva daca e identic
            if (sanitizedHtml == kdom.ContentHtml)
                return false; // nimic de salvat

            var edit = new KDomEdit
            {
                KDomId = dto.KDomId,
                UserId = userId,
                PreviousContentHtml = kdom.ContentHtml,
                NewContentHtml = sanitizedHtml,
                EditNote = dto.EditNote,
                IsMinor = dto.IsMinor,
                IsAutoSave = dto.IsAutoSave,
                EditedAt = DateTime.UtcNow
            };

            if (!dto.IsAutoSave)
            {
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    UserId = userId,
                    Action = AuditAction.EditKDom,
                    TargetType = AuditTargetType.KDom,
                    TargetId = dto.KDomId,
                    CreatedAt = DateTime.UtcNow,
                    Details = dto.EditNote ?? ""
                });
            }

            await _kdomRepository.SaveEditAsync(edit);
            await _kdomRepository.UpdateContentAsync(dto.KDomId, sanitizedHtml);

            return true;
        }

        public async Task<bool> UpdateKDomMetadataAsync(KDomUpdateMetadataDto dto, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(dto.KDomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            if (kdom.UserId != userId)
                throw new UnauthorizedAccessException("You are not the author of this K-Dom.");

            await _metadataValidator.ValidateParentAsync(dto.KDomId, dto.ParentId);

            if (kdom.Title == dto.Title &&
                kdom.Description == dto.Description &&
                kdom.Language == dto.Language &&
                kdom.Hub == dto.Hub &&
                kdom.IsForKids == dto.IsForKids &&
                kdom.Theme == dto.Theme)
            {
                return false;
            }


            var metadataEdit = new KDomMetadataEdit
            {
                PreviousParentId = kdom.ParentId,
                KDomId = kdom.Id,
                UserId = userId,
                PreviousTitle = kdom.Title,
                PreviousDescription = kdom.Description,
                PreviousLanguage = kdom.Language,
                PreviousHub = kdom.Hub,
                PreviousIsForKids = kdom.IsForKids,
                PreviousTheme = kdom.Theme,
                EditedAt = DateTime.UtcNow
            };

            await _kdomRepository.SaveMetadataEditAsync(metadataEdit);
            await _kdomRepository.UpdateMetadataAsync(dto);

            return true;
        }

        public async Task ApproveKdomAsync(string kdomId, int moderatorId)
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
            if (kdom == null) throw new Exception("K-Dom not found.");

            await _notificationService.CreateNotificationAsync(new NotificationCreateDto
            {
                UserId = kdom.UserId,
                Type = NotificationType.KDomApproved,
                Message = $"Your K-dom \"{kdom.Title}\" has been approved.",
                TriggeredByUserId = moderatorId,
                TargetType = ContentType.KDom,
                TargetId = kdomId
            });
        }

        public async Task RejectKdomAsync(string kdomId, KDomRejectDto dto, int moderatorId)
        {
            await _kdomRepository.RejectAsync(kdomId, dto.Reason);

            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = moderatorId,
                Action = AuditAction.RejectKDom,
                TargetType = AuditTargetType.KDom,
                TargetId = kdomId,
                CreatedAt = DateTime.UtcNow,
                Details = dto.Reason
            });
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null) throw new Exception("K-Dom not found.");

            await _notificationService.CreateNotificationAsync(new NotificationCreateDto
            {
                UserId = kdom.UserId,
                Type = NotificationType.KDomRejected,
                Message = $"Your K-dom \"{kdom.Title}\" has been rejected. Reason: {dto.Reason}",
                TriggeredByUserId = moderatorId,
                TargetType = ContentType.KDom,
                TargetId = kdomId
            });

        }

        public async Task RemoveCollaboratorAsync(string kdomId, int requesterId, int userIdToRemove)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            if (kdom.UserId != requesterId)
                throw new UnauthorizedAccessException("Only the owner can remove collaborators.");

            if (!kdom.Collaborators.Contains(userIdToRemove))
                throw new Exception("User is not a collaborator.");

            kdom.Collaborators.Remove(userIdToRemove);
            await _kdomRepository.UpdateCollaboratorsAsync(kdomId, kdom.Collaborators);

            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = requesterId,
                Action = AuditAction.RemoveCollaborator,
                TargetType = AuditTargetType.KDom,
                TargetId = kdomId,
                Details = $"Removed user {userIdToRemove} from collaborators.",
                CreatedAt = DateTime.UtcNow
            });

            await _notificationService.CreateNotificationAsync(new NotificationCreateDto
            {
                UserId = userIdToRemove,
                Type = NotificationType.SystemMessage,
                Message = $"You have been removed from {kdom.Title} collaborators.",
                TriggeredByUserId = requesterId,
                TargetType = ContentType.KDom,
                TargetId = kdomId
            });

        }


    }
}
