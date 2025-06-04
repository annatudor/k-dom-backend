using KDomBackend.Models.DTOs.Collaboration;
using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;
using KDomBackend.Services.Validation;

namespace KDomBackend.Services.Implementations
{
    public class KDomReadService : IKDomReadService
    {
        private readonly IKDomRepository _kdomRepository;
        private readonly IUserProfileService _userProfileService;
        private readonly IUserService _userService;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IKDomFollowRepository _kdomFollowRepository;
        private readonly IKDomEditRepository _kdomEditRepository;


        public KDomReadService(
            IKDomRepository kdomRepository,
            IUserProfileService userProfileService,
            IUserService userService,
            INotificationService notificationService,
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            IKDomFollowRepository kdomFollowRepository,
            IKDomEditRepository kdomEditRepository
            )
        {
            _kdomRepository = kdomRepository;
            _userProfileService = userProfileService;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _kdomFollowRepository = kdomFollowRepository;
            _kdomEditRepository = kdomEditRepository;
            _userService = userService;
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
                LastEditedAt = kdom.LastEditedtAt,
                Collaborators = kdom.Collaborators,
                ParentId = kdom.ParentId,
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

        public async Task<List<KDomEditReadDto>> GetEditHistoryBySlugAsync(string slug, int userId)
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug) ?? throw new Exception("K-Dom not found.");

            // Check permissions - owner, collaborators, or admins can view history
            if (kdom.UserId != userId &&
                !kdom.Collaborators.Contains(userId) &&
                !await IsUserAdminOrModeratorAsync(userId))
            {
                throw new UnauthorizedAccessException("You don't have permission to view this K-Dom's edit history.");
            }

            var edits = await _kdomRepository.GetEditsByKDomIdAsync(kdom.Id); // Use ID internally

            return edits.Select(e => new KDomEditReadDto
            {
                Id = e.Id,
                EditNote = e.EditNote ?? "",
                IsMinor = e.IsMinor,
                EditedAt = e.EditedAt
            }).ToList();
        }


        public async Task<List<KDomMetadataEditReadDto>> GetMetadataEditHistoryBySlugAsync(string slug, int userId)
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug);
            if (kdom == null)
                throw new Exception("K-Dom not found.");

            // Check permissions - owner, collaborators, or admins can view metadata history
            if (kdom.UserId != userId &&
                !kdom.Collaborators.Contains(userId) &&
                !await IsUserAdminOrModeratorAsync(userId))
            {
                throw new UnauthorizedAccessException("You don't have permission to view this K-Dom's metadata history.");
            }

            var edits = await _kdomRepository.GetMetadataEditsByKDomIdAsync(kdom.Id); // Use ID internally

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
            var ids = await _userProfileService.GetRecentlyViewedKDomIdsAsync(userId);
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

        public async Task<bool> ExistsByTitleOrSlugAsync(string title)
        {
            var slug = SlugHelper.GenerateSlug(title);
            return await _kdomRepository.ExistsByTitleOrSlugAsync(title, slug);
        }

        public async Task<List<string>> GetSimilarTitlesAsync(string title)
        {
            var similar = await _kdomRepository.FindSimilarByTitleAsync(title);
            return similar.Select(k => k.Title).ToList();
        }

        public async Task<KDomReadDto> GetKDomBySlugAsync(string slug)
        {
            var kdom = await _kdomRepository.GetBySlugAsync(slug);
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
                LastEditedAt = kdom.LastEditedtAt,
                ParentId = kdom.ParentId,
                Collaborators = kdom.Collaborators,

            };
        }

        private async Task<bool> IsUserAdminOrModeratorAsync(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            return user?.Role == "admin" || user?.Role == "moderator";
        }

    }
}
