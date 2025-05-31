using KDomBackend.Enums;
using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Models.DTOs.Post;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;
using MongoDB.Driver;

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
            var ownedKdoms = await _kdomRepository.GetOwnedOrCollaboratedByUserAsync(userId);
            var createdKDomsCount = ownedKdoms.Count(k => k.UserId == userId);
            var collaboratedKDomsCount = ownedKdoms.Count(k => k.UserId != userId && k.Collaborators.Contains(userId));

            var totalPostsCount = await GetUserPostsCountAsync(userId);
            var totalCommentsCount = await GetUserCommentsCountAsync(userId);
            var lastActivityAt = await _userRepository.GetUserLastActivityAsync(userId);

            // 4. K-Dom-uri asociate
            var ownedKDomsDisplay = ownedKdoms
                .Where(k => k.UserId == userId)
                .Select(k => new KDomDisplayDto
                {
                    Id = k.Id,
                    Title = k.Title,
                    Slug = k.Slug,
                    Description = k.Description
                }).ToList();

            var collaboratedKDomsDisplay = ownedKdoms
                .Where(k => k.UserId != userId && k.Collaborators.Contains(userId))
                .Select(k => new KDomDisplayDto
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
            var canEdit = isOwnProfile || await IsAdminOrModeratorAsync(viewerUserId);

            // 7. Date private (doar pentru owner/admin)
            List<KDomTagSearchResultDto> followedKDoms = new();
            List<KDomDisplayDto> recentlyViewedKDoms = new();

            if (isOwnProfile || await IsAdminOrModeratorAsync(viewerUserId))
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
            if (userId != requesterId && !await IsAdminOrModeratorAsync(requesterId))
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
                LastLoginAt = await GetLastLoginAsync(userId)
            };
        }

        /// <summary>
        /// Obține statistici detaliate pentru admin
        /// </summary>
        public async Task<UserDetailedStatsDto> GetUserDetailedStatsAsync(int userId, int requesterId)
        {
            // Doar admin poate accesa
            if (!await IsAdminAsync(requesterId))
            {
                throw new UnauthorizedAccessException("Admin access required.");
            }

            return new UserDetailedStatsDto
            {
                TotalKDomViews = await GetUserKDomViewsAsync(userId),
                TotalKDomEdits = await GetUserKDomEditsAsync(userId),
                TotalLikesReceived = await GetUserLikesReceivedAsync(userId),
                TotalLikesGiven = await GetUserLikesGivenAsync(userId),
                TotalCommentsReceived = await GetUserCommentsReceivedAsync(userId),
                TotalFlagsReceived = await _userRepository.GetUserFlagsReceivedAsync(userId),
                ActivityByMonth = await _userRepository.GetUserActivityByMonthAsync(userId),
                RecentActions = await _userRepository.GetUserRecentActionsAsync(userId)
            };
        }

        // Helper methods
        private async Task<int> GetUserPostsCountAsync(int userId)
        {
            return await _postRepository.GetPostCountByUserAsync(userId);
        }

        private async Task<int> GetUserCommentsCountAsync(int userId)
        {
            return await _commentRepository.GetCommentCountByUserAsync(userId);
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

        private async Task<bool> IsAdminOrModeratorAsync(int? userId)
        {
            if (!userId.HasValue) return false;
            var user = await _userRepository.GetByIdAsync(userId.Value);
            return user?.Role == "admin" || user?.Role == "moderator";
        }

        private async Task<bool> IsAdminAsync(int? userId)
        {
            if (!userId.HasValue) return false;
            var user = await _userRepository.GetByIdAsync(userId.Value);
            return user?.Role == "admin";
        }

        // Implementează metodele placeholder
        private async Task<DateTime?> GetLastLoginAsync(int userId)
        {
            return await _userRepository.GetLastLoginAsync(userId);
        }

        private async Task<int> GetUserKDomViewsAsync(int userId)
        {
            return await _kdomRepository.GetUserKDomViewsAsync(userId);
        }

        private async Task<int> GetUserKDomEditsAsync(int userId)
        {
            return await _kdomRepository.GetUserKDomEditsCountAsync(userId);
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

            // Numără comentariile pe postări
            var commentsOnPosts = 0;
            if (userPostIds.Any())
            {
                var filter = Builders<Comment>.Filter.And(
                    Builders<Comment>.Filter.Eq(c => c.TargetType, CommentTargetType.Post),
                    Builders<Comment>.Filter.In(c => c.TargetId, userPostIds),
                    Builders<Comment>.Filter.Ne(c => c.UserId, userId) // Exclude propriile comentarii
                );
                commentsOnPosts = (int)await _commentRepository._collection.CountDocumentsAsync(filter);
            }

            // Numără comentariile pe K-Dom-uri
            var commentsOnKDoms = 0;
            if (userKDomIds.Any())
            {
                var kdomComments = await _commentRepository.GetCommentsOnUserKDomsAsync(userKDomIds);
                commentsOnKDoms = kdomComments.Count(c => c.UserId != userId);
            }

            return commentsOnPosts + commentsOnKDoms;
        }

        // Metodele existente rămân la fel
        public async Task<UserProfileReadDto> GetUserProfileAsync(int userId)
        {
            return await GetEnhancedUserProfileAsync(userId);
        }

        

        public async Task AddRecentlyViewedKDomAsync(int userId, string kdomId)
        {
            await _profileRepository.AddRecentlyViewedKDomAsync(userId, kdomId);
        }

        public async Task<List<string>> GetRecentlyViewedKDomIdsAsync(int userId)
        {
            return await _profileRepository.GetRecentlyViewedKDomIdsAsync(userId);
        }

        public async Task<bool> CanUserUpdateProfileAsync(int currentUserId, int targetUserId)
        {
            if (currentUserId == targetUserId) return true;
            return await IsAdminOrModeratorAsync(currentUserId);
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