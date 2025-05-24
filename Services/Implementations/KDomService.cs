using KDomBackend.Enums;
using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.Collaboration;
using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Models.DTOs.Notification;
using KDomBackend.Models.Entities;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Implementations;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;
using KDomBackend.Services.Validation;

namespace KDomBackend.Services.Implementations
{
    public class KDomService : IKDomService
    {
        private readonly IKDomRepository _kdomRepository;
        private readonly IUserService _userService;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly KDomValidator _validator;
        private readonly KDomMetadataValidator _metadataValidator;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IKDomFollowRepository _kdomFollowRepository;
        private readonly IKDomEditRepository _kdomEditRepository;


        public KDomService(IKDomRepository kdomRepository, 
            IUserService userService,
            IAuditLogRepository auditLogRepository, 
            INotificationService notificationService, 
            IUserRepository userRepository, 
            KDomValidator validator,
            KDomMetadataValidator metadataValidator,
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            IKDomFollowRepository kdomFollowRepository,
            IKDomEditRepository kdomEditRepository
            )
        {
            _kdomRepository = kdomRepository;
            _userService = userService;
            _auditLogRepository = auditLogRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _validator = validator;
            _metadataValidator = metadataValidator;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _kdomFollowRepository = kdomFollowRepository;
            _kdomEditRepository = kdomEditRepository;
        }


        public async Task CreateKDomAsync(KDomCreateDto dto, int userId)
        {
            var sanitizedHtml = HtmlSanitizerHelper.Sanitize(dto.ContentHtml);
            await _validator.CheckDuplicateOrSuggestAsync(dto.Title);

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
            {   PreviousParentId = kdom.ParentId,
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
                PreviousParentId = e.PreviousParentId,
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

        public async Task<List<KDomReadDto>> GetChildrenAsync(string parentId)
        {
            var children = await _kdomRepository.GetChildrenByParentIdAsync(parentId);

            var result = new List<KDomReadDto>();

            foreach (var kdom in children)
            {
                var username = await _userService.GetUsernameByUserIdAsync(kdom.UserId);

                result.Add(new KDomReadDto
                {
                    Id = kdom.Id,
                    Title = kdom.Title,
                    Slug = kdom.Slug,
                    Description = kdom.Description,
                    Hub = kdom.Hub,
                    Language = kdom.Language,
                    Theme = kdom.Theme,
                    IsForKids = kdom.IsForKids,
                    ContentHtml = kdom.ContentHtml,
                    AuthorUsername = username,
                    UserId = kdom.UserId,
                    CreatedAt = kdom.CreatedAt,
                    UpdatedAt = kdom.UpdatedAt,
                    LastEditedAt = kdom.LastEditedtAt
                });
            }

            return result;
        }

        public async Task<KDomReadDto?> GetParentAsync(string childId)
        {
            var parent = await _kdomRepository.GetParentAsync(childId);
            if (parent == null)
                return null;

            var username = await _userService.GetUsernameByUserIdAsync(parent.UserId);

            return new KDomReadDto
            {
                Id = parent.Id,
                Title = parent.Title,
                Slug = parent.Slug,
                Description = parent.Description,
                Hub = parent.Hub,
                Language = parent.Language,
                Theme = parent.Theme,
                IsForKids = parent.IsForKids,
                ContentHtml = parent.ContentHtml,
                AuthorUsername = username,
                UserId = parent.UserId,
                CreatedAt = parent.CreatedAt,
                UpdatedAt = parent.UpdatedAt,
                LastEditedAt = parent.LastEditedtAt
            };
        }

        public async Task<List<KDomReadDto>> GetSiblingsAsync(string kdomId)
        {
            var siblings = await _kdomRepository.GetSiblingsAsync(kdomId);

            var result = new List<KDomReadDto>();
            foreach (var k in siblings)
            {
                var username = await _userService.GetUsernameByUserIdAsync(k.UserId);

                result.Add(new KDomReadDto
                {
                    Id = k.Id,
                    Title = k.Title,
                    Slug = k.Slug,
                    Description = k.Description,
                    Hub = k.Hub,
                    Language = k.Language,
                    Theme = k.Theme,
                    IsForKids = k.IsForKids,
                    ContentHtml = k.ContentHtml,
                    AuthorUsername = username,
                    UserId = k.UserId,
                    CreatedAt = k.CreatedAt,
                    UpdatedAt = k.UpdatedAt,
                    LastEditedAt = k.LastEditedtAt
                });
            }

            return result;
        }

        public async Task<List<CollaboratorReadDto>> GetCollaboratorsAsync(string kdomId, int requesterId)
        {
            var kdom = await _kdomRepository.GetByIdAsync(kdomId);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            if (kdom.UserId != requesterId)
                throw new UnauthorizedAccessException("Only the owner can view collaborators.");

            var result = new List<CollaboratorReadDto>();

            foreach (var userId in kdom.Collaborators)
            {
                var username = await _userService.GetUsernameByUserIdAsync(userId);
                result.Add(new CollaboratorReadDto
                {
                    UserId = userId,
                    Username = username ?? "unknown"
                });
            }

            return result;
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

        public async Task<List<KDomTagSearchResultDto>> SearchTagOrSlugAsync(string query)
        {
            var results = await _kdomRepository.SearchTitleOrSlugByQueryAsync(query);

            return results.Select(k => new KDomTagSearchResultDto
            {
                Id = k.Id,
                Title = k.Title,
                Slug = k.Slug
            }).ToList();
        }
        public async Task<List<KDomTrendingDto>> GetTrendingKdomsAsync(int days = 7)
        {
            var postScores = await _postRepository.GetRecentTagCountsAsync(days);
            var slugs = postScores.Keys.ToList();
            var kdoms = await _kdomRepository.GetBySlugsAsync(slugs);

            var result = kdoms.Select(k => new KDomTrendingDto
            {
                Id = k.Id,
                Title = k.Title,
                Slug = k.Slug,
                PostScore = postScores.TryGetValue(k.Slug, out var count) ? count : 0
            }).ToList();

            // urmează să adăugăm comentarii, follows, edits
            var commentScores = await _commentRepository.CountRecentCommentsByKDomAsync(days);

            foreach (var item in result)
            {
                item.CommentScore = commentScores.TryGetValue(item.Id, out var count) ? count : 0;
            }

            var followScores = await _kdomFollowRepository.CountRecentFollowsAsync(days);

            foreach (var item in result)
            {
                item.FollowScore = followScores.TryGetValue(item.Id, out var count) ? count : 0;
            }

            var editScores = await _kdomRepository.CountRecentEditsAsync(days);

            foreach (var item in result)
            {
                item.EditScore = editScores.TryGetValue(item.Id, out var count) ? count : 0;
            }



            return result;
        }

        public async Task<List<KDomTagSearchResultDto>> GetSuggestedKdomsAsync(int userId, int limit = 10)
        {
            var followedSlugs = await _kdomFollowRepository.GetFollowedSlugsAsync(userId);

            
            var recentPosts = await _postRepository.GetRecentPostsByUserAsync(userId, 30);
            var postSlugs = recentPosts.SelectMany(p => p.Tags).Distinct();

            
            var commentedKDomIds = await _commentRepository.GetCommentedKDomIdsByUserAsync(userId);
            var editedKDomIds = await _kdomEditRepository.GetEditedKDomIdsByUserAsync(userId);

            
            var allKDomIds = commentedKDomIds.Concat(editedKDomIds).Distinct();
            var kdomsFromIds = await _kdomRepository.GetByIdsAsync(allKDomIds);
            var commentSlugs = kdomsFromIds.Select(k => k.Slug).ToList();

            
            var suggestedSlugs = postSlugs
                .Concat(commentSlugs)
                .Distinct()
                .Except(followedSlugs)
                .Take(limit)
                .ToList();

            var suggestedKdoms = await _kdomRepository.GetBySlugsAsync(suggestedSlugs);

            return suggestedKdoms.Select(k => new KDomTagSearchResultDto
            {
                Id = k.Id,
                Title = k.Title,
                Slug = k.Slug
            }).ToList();
        }

        public async Task<List<KDomDisplayDto>> GetKdomsForUserAsync(int userId)
        {
            var kdoms = await _kdomRepository.GetOwnedOrCollaboratedByUserAsync(userId);

            return kdoms.Select(k => new KDomDisplayDto
            {
                Id = k.Id,
                Title = k.Title,
                Slug = k.Slug,
                Description = k.Description
            }).ToList();
        }
        public async Task<List<KDomDisplayDto>> GetRecentlyViewedKdomsAsync(int userId)
        {
            var ids = await _userService.GetRecentlyViewedKDomIdsAsync(userId);
            if (!ids.Any()) return new();

            var kdoms = await _kdomRepository.GetByIdsAsync(ids);

            return kdoms.Select(k => new KDomDisplayDto
            {
                Id = k.Id,
                Title = k.Title,
                Slug = k.Slug,
                Description = k.Description
            }).ToList();
        }


    }
}
