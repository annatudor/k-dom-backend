using KDomBackend.Enums;
using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Models.DTOs.Post;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class EnhancedUserProfileService : IUserProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileRepository _profileRepository;
        private readonly IFollowRepository _followRepository;
        private readonly IKDomRepository _kdomRepository;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IKDomFollowRepository _kdomFollowRepository;

        public EnhancedUserProfileService(
            IUserRepository userRepository,
            IUserProfileRepository profileRepository,
            IFollowRepository followRepository,
            IKDomRepository kdomRepository,
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            IKDomFollowRepository kdomFollowRepository)
        {
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _followRepository = followRepository;
            _kdomRepository = kdomRepository;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _kdomFollowRepository = kdomFollowRepository;
        }

        /// <summary>
        /// Obține profilul complet al unui utilizator cu toate datele
        /// </summary>
        public async Task<UserProfileReadDto> GetEnhancedUserProfileAsync(int userId, int? viewerUserId = null)
        {
            // 1. Informații de bază
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var profile = await _profileRepository.GetProfileByUserIdAsync(userId);

            // 2. Statistici sociale
            var followersCount = await _followRepository.GetFollowersCountAsync(userId);
            var followingCount = await _followRepository.GetFollowingCountAsync(userId);

            bool? isFollowedByCurrentUser = null;
            if (viewerUserId.HasValue && viewerUserId.Value != userId)
            {
                isFollowedByCurrentUser = await _followRepository.ExistsAsync(viewerUserId.Value, userId);
            }

            // 3. Statistici de activitate și contribuții
            var createdKDomsCount = await _kdomRepository.GetCreatedKDomsCountByUserAsync(userId);
            var collaboratedKDomsCount = await _kdomRepository.GetCollaboratedKDomsCountByUserAsync(userId);
            var totalPostsCount = await _postRepository.GetPostCountByUserAsync(userId);
            var totalCommentsCount = await _commentRepository.GetCommentCountByUserAsync(userId);
            var lastActivityAt = await _userRepository.GetUserLastActivityAsync(userId);

            // 4. K-Dom-uri asociate
            var ownedKDoms = await _kdomRepository.GetKDomsByUserAsync(userId, false); // Doar create
            var collaboratedKDoms = await GetCollaboratedKDomsAsync(userId);

            var ownedKDomsDisplay = ownedKDoms.Select(k => new KDomDisplayDto
            {
                Id = k.Id,
                Title = k.Title,
                Slug = k.Slug,
                Description = k.Description
            }).ToList();

            var collaboratedKDomsDisplay = collaboratedKDoms.Select(k => new KDomDisplayDto
            {
                Id = k.Id,
                Title = k.Title,
                Slug = k.Slug,
                Description = k.Description
            }).ToList();

            // 5. Postări recente (ultimele 5)
            var recentPosts = await GetUserRecentPostsAsync(userId, 5);

            // 6. Context și permisiuni
            var isOwnProfile = viewerUserId.HasValue && viewerUserId.Value == userId;
            var canEdit = isOwnProfile || await IsUserAdminOrModeratorAsync(viewerUserId ?? 0);

            // 7. Date private (doar pentru owner/admin)
            List<KDomTagSearchResultDto> followedKDoms = new();
            List<KDomDisplayDto> recentlyViewedKDoms = new();

            if (isOwnProfile || await IsUserAdminAsync(viewerUserId ?? 0))
            {
                // Followed K-Doms
                var followedKDomIds = await _kdomFollowRepository.GetFollowedKDomIdsAsync(userId);
                var followedKDomsData = await _kdomRepository.GetByIdsAsync(followedKDomIds);
                followedKDoms = followedKDomsData.Select(k => new KDomTagSearchResultDto
                {
                    Id = k.Id,
                    Title = k.Title,
                    Slug = k.Slug
                }).ToList();

                // Recently viewed K-Doms
                var recentlyViewedIds = await _profileRepository.GetRecentlyViewedKDomIdsAsync(userId);
                var recentlyViewedData = await _kdomRepository.GetByIdsAsync(recentlyViewedIds);
                recentlyViewedKDoms = recentlyViewedData.Select(k => new KDomDisplayDto
                {
                    Id = k.Id,
                    Title = k.Title,
                    Slug = k.Slug,
                    Description = k.Description
                }).ToList();
            }

            return new UserProfileReadDto
            {
                // Informații de bază
                UserId = user.Id,
                Username = user.Username,
                Nickname = profile?.Nickname ?? "",
                AvatarUrl = profile?.AvatarUrl ?? "",
                Bio = profile?.Bio ?? "",
                ProfileTheme = profile?.ProfileTheme ?? ProfileTheme.Default,
                JoinedAt = user.CreatedAt,

                // Statistici sociale
                FollowersCount = followersCount,
                FollowingCount = followingCount,
                IsFollowedByCurrentUser = isFollowedByCurrentUser,

                // Activitate și contribuții
                CreatedKDomsCount = createdKDomsCount,
                CollaboratedKDomsCount = collaboratedKDomsCount,
                TotalPostsCount = totalPostsCount,
                TotalCommentsCount = totalCommentsCount,
                LastActivityAt = lastActivityAt,

                // K-Dom-uri asociate
                OwnedKDoms = ownedKDomsDisplay,
                CollaboratedKDoms = collaboratedKDomsDisplay,
                FollowedKDoms = followedKDoms,
                RecentlyViewedKDoms = recentlyViewedKDoms,

                // Postări
                RecentPosts = recentPosts,

                // Context
                IsOwnProfile = isOwnProfile,
                CanEdit = canEdit
            };
        }

        /// <summary>
        /// Obține informații private pentru user/admin
        /// </summary>
        public async Task<UserPrivateInfoDto> GetUserPrivateInfoAsync(int userId, int requesterId)
        {
            // Verifică permisiunile
            if (userId != requesterId && !await IsUserAdminOrModeratorAsync(requesterId))
            {
                throw new UnauthorizedAccessException("Access denied to private information.");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            return new UserPrivateInfoDto
            {
                Email = user.Email,
                Role = user.Role ?? "user",
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                Provider = user.Provider,
                LastLoginAt = await _userRepository.GetLastLoginAsync(userId)
            };
        }

        /// <summary>
        /// Obține statistici detaliate pentru admin
        /// </summary>
        public async Task<UserDetailedStatsDto> GetUserDetailedStatsAsync(int userId, int requesterId)
        {
            // Doar admin poate accesa
            if (!await IsUserAdminAsync(requesterId))
            {
                throw new UnauthorizedAccessException("Admin access required.");
            }

            return new UserDetailedStatsDto
            {
                TotalKDomViews = await _kdomRepository.GetUserKDomViewsAsync(userId),
                TotalKDomEdits = await _kdomRepository.GetUserKDomEditsCountAsync(userId),
                TotalLikesReceived = await GetUserLikesReceivedAsync(userId),
                TotalLikesGiven = await GetUserLikesGivenAsync(userId),
                TotalCommentsReceived = await GetUserCommentsReceivedAsync(userId),
                TotalFlagsReceived = await _userRepository.GetUserFlagsReceivedAsync(userId),
                ActivityByMonth = await _userRepository.GetUserActivityByMonthAsync(userId),
                RecentActions = await _userRepository.GetUserRecentActionsAsync(userId)
            };
        }

        /// <summary>
        /// Verifică dacă utilizatorul este admin
        /// </summary>
        public async Task<bool> IsUserAdminAsync(int userId)
        {
            if (userId <= 0) return false;
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.Role == "admin";
        }

        /// <summary>
        /// Verifică dacă utilizatorul este admin sau moderator
        /// </summary>
        public async Task<bool> IsUserAdminOrModeratorAsync(int userId)
        {
            if (userId <= 0) return false;
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.Role == "admin" || user?.Role == "moderator";
        }

        // Helper methods
        private async Task<List<KDom>> GetCollaboratedKDomsAsync(int userId)
        {
            var allKDoms = await _kdomRepository.GetKDomsByUserAsync(userId, true);
            return allKDoms.Where(k => k.UserId != userId && k.Collaborators.Contains(userId)).ToList();
        }

        private async Task<List<PostReadDto>> GetUserRecentPostsAsync(int userId, int limit)
        {
            var posts = await _postRepository.GetPostsByUserAsync(userId, limit);
            var result = new List<PostReadDto>();

            foreach (var post in posts)
            {
                var username = await _userRepository.GetUsernameByUserIdAsync(post.UserId);
                result.Add(new PostReadDto
                {
                    Id = post.Id,
                    UserId = post.UserId,
                    Username = username,
                    ContentHtml = post.ContentHtml,
                    Tags = post.Tags,
                    CreatedAt = post.CreatedAt,
                    IsEdited = post.IsEdited,
                    EditedAt = post.EditedAt,
                    LikeCount = post.Likes.Count
                });
            }

            return result;
        }

        private async Task<int> GetUserLikesReceivedAsync(int userId)
        {
            var postLikes = await _postRepository.GetTotalLikesReceivedByUserPostsAsync(userId);
            var commentLikes = await _commentRepository.GetTotalLikesReceivedByUserCommentsAsync(userId);
            return postLikes + commentLikes;
        }

        private async Task<int> GetUserLikesGivenAsync(int userId)
        {
            var postLikes = await _postRepository.GetTotalLikesGivenByUserAsync(userId);
            var commentLikes = await _commentRepository.GetTotalLikesGivenByUserAsync(userId);
            return postLikes + commentLikes;
        }

        private async Task<int> GetUserCommentsReceivedAsync(int userId)
        {
            // Obține toate postările user-ului
            var userPostIds = await _postRepository.GetUserPostIdsAsync(userId);

            // Obține toate K-Dom-urile user-ului
            var userKDomIds = await _kdomRepository.GetUserKDomIdsAsync(userId, false);

            // Calculează comentariile primite pe postări
            var commentsOnPosts = 0;
            if (userPostIds.Any())
            {
                // Implementăm prin service - contorizăm comentariile pe postările user-ului
                // Aceasta va fi implementată când adăugăm metoda în CommentRepository
                commentsOnPosts = await GetCommentsCountOnPostsAsync(userPostIds, userId);
            }

            // Calculează comentariile primite pe K-Dom-uri
            var commentsOnKDoms = 0;
            if (userKDomIds.Any())
            {
                var kdomComments = await _commentRepository.GetCommentsOnUserKDomsAsync(userKDomIds);
                commentsOnKDoms = kdomComments.Count(c => c.UserId != userId);
            }

            return commentsOnPosts + commentsOnKDoms;
        }

        private async Task<int> GetCommentsCountOnPostsAsync(List<string> postIds, int excludeUserId)
        {
            return await _commentRepository.GetCommentsCountOnPostsAsync(postIds, excludeUserId);
        }

        // Implementările metodelor din interfața de bază
        public async Task<UserProfileReadDto> GetUserProfileAsync(int userId)
        {
            return await GetEnhancedUserProfileAsync(userId);
        }

        public async Task UpdateProfileAsync(int userId, UserProfileUpdateDto dto)
        {
            // Verifică dacă utilizatorul există
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            // Validări suplimentare pentru date
            if (!string.IsNullOrEmpty(dto.Nickname) && dto.Nickname.Length > 50)
                throw new Exception("Nickname cannot exceed 50 characters.");

            if (!string.IsNullOrEmpty(dto.Bio) && dto.Bio.Length > 500)
                throw new Exception("Bio cannot exceed 500 characters.");

            if (!string.IsNullOrEmpty(dto.AvatarUrl) && !Uri.IsWellFormedUriString(dto.AvatarUrl, UriKind.Absolute))
                throw new Exception("Avatar URL must be a valid URL.");

            // Obține profilul existent sau creează unul nou
            var profile = await _profileRepository.GetProfileByUserIdAsync(userId);

            if (profile == null)
            {
                // Creează profil nou
                profile = new UserProfile
                {
                    UserId = userId,
                    Nickname = dto.Nickname ?? "",
                    Bio = dto.Bio ?? "",
                    ProfileTheme = dto.ProfileTheme,
                    AvatarUrl = dto.AvatarUrl ?? "",
                    JoinedAt = DateTime.UtcNow
                };

                await _profileRepository.CreateAsync(profile);
            }
            else
            {
                // Actualizează profilul existent
                profile.Nickname = dto.Nickname ?? profile.Nickname;
                profile.Bio = dto.Bio ?? profile.Bio;
                profile.ProfileTheme = dto.ProfileTheme;
                profile.AvatarUrl = dto.AvatarUrl ?? profile.AvatarUrl;

                await _profileRepository.UpdateAsync(profile);
            }
        }

        public async Task AddRecentlyViewedKDomAsync(int userId, string kdomId)
        {
            if (string.IsNullOrEmpty(kdomId))
                throw new ArgumentException("K-Dom ID cannot be null or empty.");

            await _profileRepository.AddRecentlyViewedKDomAsync(userId, kdomId);
        }

        public async Task<List<string>> GetRecentlyViewedKDomIdsAsync(int userId)
        {
            return await _profileRepository.GetRecentlyViewedKDomIdsAsync(userId);
        }

        public async Task<bool> CanUserUpdateProfileAsync(int currentUserId, int targetUserId)
        {
            if (currentUserId == targetUserId) return true;
            return await IsUserAdminOrModeratorAsync(currentUserId);
        }

        public async Task ValidateUpdatePermissionsAsync(int currentUserId, int targetUserId)
        {
            if (!await CanUserUpdateProfileAsync(currentUserId, targetUserId))
            {
                throw new UnauthorizedAccessException("You don't have permission to update this profile.");
            }
        }
    }
}