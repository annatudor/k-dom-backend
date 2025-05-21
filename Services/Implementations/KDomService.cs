using KDomBackend.Enums;
using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Models.DTOs.Notification;
using KDomBackend.Models.Entities;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Implementations;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class KDomService : IKDomService
    {
        private readonly IKDomRepository _kdomRepository;
        private readonly IUserService _userService;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;

        public KDomService(IKDomRepository kdomRepository, 
            IUserService userService,
            IAuditLogRepository auditLogRepository, 
            INotificationService notificationService, 
            IUserRepository userRepository)
        {
            _kdomRepository = kdomRepository;
            _userService = userService;
            _auditLogRepository = auditLogRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
        }


        public async Task CreateKDomAsync(KDomCreateDto dto, int userId)
        {
            var sanitizedHtml = HtmlSanitizerHelper.Sanitize(dto.ContentHtml);

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
                CreatedAt = DateTime.UtcNow
            };

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


        public async Task<KDomReadDto> GetKDomByIdAsync(string id)
        {
            var kdom = await _kdomRepository.GetByIdAsync(id);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            var username = await _userService.GetUsernameByUserIdAsync(kdom.UserId);


            return new KDomReadDto
            {
                Id = kdom.Id,
                Title = kdom.Title,
                Slug = kdom.Slug,
                Description = kdom.Description,
                Hub = kdom.Hub,
                Theme = kdom.Theme,
                ContentHtml = kdom.ContentHtml,
                Language = kdom.Language,
                IsForKids = kdom.IsForKids,
                UserId = kdom.UserId,
                AuthorUsername = username,
                CreatedAt = kdom.CreatedAt,
                UpdatedAt = kdom.UpdatedAt,
                LastEditedAt = kdom.LastEditedtAt
            };
        }

        public async Task<List<KDomEditReadDto>> GetEditHistoryAsync(string kdomId, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId) ?? throw new Exception("K-Dom not found.");
            if (kdom.UserId != userId)
                throw new UnauthorizedAccessException("You are not the author of this K-Dom.");

            var edits = await _kdomRepository.GetEditsByKDomIdAsync(kdomId);

            return edits.Select(e => new KDomEditReadDto
            {
                Id = e.Id,
                EditNote = e.EditNote ?? "",
                IsMinor = e.IsMinor,
                EditedAt = e.EditedAt
            }).ToList();
        }

        public async Task<List<KDomMetadataEditReadDto>> GetMetadataEditHistoryAsync(string kdomId, int userId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            if (kdom.UserId != userId)
                throw new UnauthorizedAccessException("You are not the owner of this K-Dom.");

            var edits = await _kdomRepository.GetMetadataEditsByKDomIdAsync(kdomId);

            return edits.Select(e => new KDomMetadataEditReadDto
            {
                Id = e.Id,
                PreviousTitle = e.PreviousTitle,
                PreviousDescription = e.PreviousDescription,
                PreviousLanguage = e.PreviousLanguage,
                PreviousHub = e.PreviousHub,
                PreviousIsForKids = e.PreviousIsForKids,
                PreviousTheme = e.PreviousTheme,
                EditedAt = e.EditedAt
            }).ToList();
        }

        public async Task<List<KDomReadDto>> GetPendingKdomsAsync()
        {
            var pending = await _kdomRepository.GetPendingKdomsAsync();
            var result = new List<KDomReadDto>();

            foreach (var k in pending)
            {
                var username = await _userService.GetUsernameByUserIdAsync(k.UserId);

                result.Add(new KDomReadDto
                {
                    Id = k.Id,
                    Title = k.Title,
                    Description = k.Description,
                    UserId = k.UserId,
                    AuthorUsername = username,
                    CreatedAt = k.CreatedAt,
                    IsForKids = k.IsForKids,
                    ContentHtml = k.ContentHtml
                });
            }

            return result;
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


    }
}
