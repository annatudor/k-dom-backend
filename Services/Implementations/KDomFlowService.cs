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
        private readonly IKDomPermissionService _permissionService; 
        private readonly KDomValidator _validator;
        private readonly KDomMetadataValidator _metadataValidator;
        private readonly ILogger<KDomFlowService> _logger;

        public KDomFlowService(
            IKDomRepository kdomRepository,
            IUserService userService,
            IAuditLogRepository auditLogRepository,
            INotificationService notificationService,
            IUserRepository userRepository,
            IKDomPermissionService permissionService, 
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
            _permissionService = permissionService; 
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
                ParentId = dto.ParentId,
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

        // Existing method (keep for backward compatibility) - assumes KDomSlug contains an ID
        public async Task<bool> EditKDomAsync(KDomEditDto dto, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(dto.KDomSlug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return await PerformEditAsync(kdom, dto, userId);
        }

        // NEW: Slug-based edit method
        public async Task<bool> EditKDomBySlugAsync(KDomEditDto dto, int userId)
        {
            var kdom = await _kdomRepository.GetBySlugAsync(dto.KDomSlug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return await PerformEditAsync(kdom, dto, userId);
        }

        // Existing metadata update method (keep for backward compatibility) - assumes KDomSlug contains an ID
        public async Task<bool> UpdateKDomMetadataByIdAsync(string kdomId, KDomUpdateMetadataDto dto, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return await PerformMetadataUpdateAsync(kdom, dto, userId);
        }

        public async Task<bool> UpdateKDomMetadataBySlugAsync(string slug, KDomUpdateMetadataDto dto, int userId)
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            return await PerformMetadataUpdateAsync(kdom, dto, userId);
        }


        // COMMON EDIT LOGIC (now using Permission Service)

        /// <summary>
        /// Common edit logic extracted to avoid duplication
        /// </summary>
        private async Task<bool> PerformEditAsync(KDom kdom, KDomEditDto dto, int userId)
        {
           
            await _permissionService.EnsureUserCanEditKDomAsync(kdom, userId);

            var sanitizedHtml = HtmlSanitizerHelper.Sanitize(dto.ContentHtml);

            // Don't save if content is identical
            if (sanitizedHtml == kdom.ContentHtml)
                return false; // nothing to save

            var edit = new KDomEdit
            {
                KDomId = kdom.Id,
                UserId = userId,
                PreviousContentHtml = kdom.ContentHtml,
                NewContentHtml = sanitizedHtml,
                EditNote = dto.EditNote,
                IsMinor = dto.IsMinor,
                IsAutoSave = dto.IsAutoSave,
                EditedAt = DateTime.UtcNow
            };

            // Only create audit log for manual saves
            if (!dto.IsAutoSave)
            {
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    UserId = userId,
                    Action = AuditAction.EditKDom,
                    TargetType = AuditTargetType.KDom,
                    TargetId = kdom.Id,
                    CreatedAt = DateTime.UtcNow,
                    Details = dto.EditNote ?? ""
                });
            }

            await _kdomRepository.SaveEditAsync(edit);
            await _kdomRepository.UpdateContentAsync(kdom.Id, sanitizedHtml);

            return true;
        }

        /// <summary>
        /// Common metadata update logic (now using Permission Service)
        /// </summary>
        private async Task<bool> PerformMetadataUpdateAsync(KDom kdom, KDomUpdateMetadataDto dto, int userId)
        {
            // Check permissions using the permission service
            await _permissionService.EnsureUserCanEditMetadataAsync(kdom, userId, "update metadata for");

            // Procesează parentId - convertește string gol în null
            string? actualParentId = string.IsNullOrWhiteSpace(dto.ParentId) ? null : dto.ParentId;

            // Validează parentId doar dacă nu este null/empty
            if (!string.IsNullOrEmpty(actualParentId))
            {
                await _metadataValidator.ValidateParentAsync(kdom.Id, actualParentId);
            }

            // Check if anything actually changed
            if (kdom.Title == dto.Title &&
                kdom.Description == dto.Description &&
                kdom.Language == dto.Language &&
                kdom.Hub == dto.Hub &&
                kdom.IsForKids == dto.IsForKids &&
                kdom.Theme == dto.Theme &&
                kdom.ParentId == actualParentId) // Folosește actualParentId
            {
                return false; // No changes
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

            // Update the DTO to use the correct ID for the repository method
            var updateDto = new KDomUpdateMetadataDto
            {
                KDomSlug = kdom.Id, // Use ID for the repository call
                Title = dto.Title,
                ParentId = actualParentId, // Folosește actualParentId
                Description = dto.Description,
                Hub = dto.Hub,
                Language = dto.Language,
                IsForKids = dto.IsForKids,
                Theme = dto.Theme
            };

            await _kdomRepository.UpdateMetadataAsync(updateDto);

            return true;
        }

        // OTHER EXISTING METHODS (now using Permission Service)

        public async Task CreateSubKDomAsync(string parentId, KDomSubCreateDto dto, int userId)
        {
            var parent = await _kdomRepository.GetByIdAsync(parentId);
            if (parent == null)
                throw new Exception("Parent not found.");

            // Use permission service
            await _permissionService.EnsureUserCanEditKDomAsync(parent, userId, "create a sub-page for");

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

       
        public async Task RemoveCollaboratorAsync(string kdomId, int requesterId, int userIdToRemove)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            // Use permission service to check if user is owner
            _permissionService.EnsureUserIsOwner(kdom, requesterId, "remove collaborators from");

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