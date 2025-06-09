using KDomBackend.Enums;
using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Models.DTOs.Post;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileRepository _profileRepository;
        private readonly IFollowRepository _followRepository;
        private readonly IKDomRepository _kdomRepository;
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IKDomFollowRepository _kdomFollowRepository;
        private readonly IViewTrackingService _viewTrackingService;


        public UserProfileService(
            IUserRepository userRepository,
            IUserProfileRepository profileRepository,
            IFollowRepository followRepository,
            IKDomRepository kdomRepository,
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            IKDomFollowRepository kdomFollowRepository,
            IViewTrackingService viewTrackingService)
        {
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _followRepository = followRepository;
            _kdomRepository = kdomRepository;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _kdomFollowRepository = kdomFollowRepository;
            _viewTrackingService = viewTrackingService;
        }

        /// <summary>
        /// Obține profilul complet al unui utilizator cu toate datele
        /// </summary>
        public async Task<UserProfileReadDto> GetUserProfileAsync(int userId, int? viewerUserId = null)
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

            var recentPosts = await GetUserRecentPostsAsync(userId, 5);

            var isOwnProfile = viewerUserId.HasValue && viewerUserId.Value == userId;
            var canEdit = isOwnProfile || await IsUserAdminOrModeratorAsync(viewerUserId ?? 0);

            List<KDomTagSearchResultDto> followedKDoms = new();
            List<KDomDisplayDto> recentlyViewedKDoms = new();

            if (isOwnProfile || await IsUserAdminAsync(viewerUserId ?? 0))
            {
                var followedKDomIds = await _kdomFollowRepository.GetFollowedKDomIdsAsync(userId);
                var followedKDomsData = await _kdomRepository.GetByIdsAsync(followedKDomIds);
                followedKDoms = followedKDomsData.Select(k => new KDomTagSearchResultDto
                {
                    Id = k.Id,
                    Title = k.Title,
                    Slug = k.Slug
                }).ToList();

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
           
                UserId = user.Id,
                Username = user.Username,
                Nickname = profile?.Nickname ?? "",
                AvatarUrl = profile?.AvatarUrl ?? "",
                Bio = profile?.Bio ?? "",
                ProfileTheme = profile?.ProfileTheme ?? ProfileTheme.Default,
                JoinedAt = user.CreatedAt,

                FollowersCount = followersCount,
                FollowingCount = followingCount,
                IsFollowedByCurrentUser = isFollowedByCurrentUser,

                CreatedKDomsCount = createdKDomsCount,
                CollaboratedKDomsCount = collaboratedKDomsCount,
                TotalPostsCount = totalPostsCount,
                TotalCommentsCount = totalCommentsCount,
                LastActivityAt = lastActivityAt,

                OwnedKDoms = ownedKDomsDisplay,
                CollaboratedKDoms = collaboratedKDomsDisplay,
                FollowedKDoms = followedKDoms,
                RecentlyViewedKDoms = recentlyViewedKDoms,

                
                RecentPosts = recentPosts,

                IsOwnProfile = isOwnProfile,
                CanEdit = canEdit
            };
        }

       
        public async Task<UserPrivateInfoDto> GetUserPrivateInfoAsync(int userId, int requesterId)
        {
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


        public async Task<UserDetailedStatsDto> GetUserDetailedStatsAsync(int userId, int requesterId)
        {
            try
            {
                // Only admin can view detailed stats
                if (!await IsUserAdminAsync(requesterId))
                {
                    throw new UnauthorizedAccessException("Admin access required.");
                }

                // Get user to ensure they exist
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new Exception("User not found.");
                }

                Console.WriteLine($"[DEBUG] Getting detailed stats for user {userId}");

                // Initialize with safe defaults
                var stats = new UserDetailedStatsDto
                {
                    TotalKDomViews = 0,
                    TotalKDomEdits = 0,
                    TotalLikesReceived = 0,
                    TotalLikesGiven = 0,
                    TotalCommentsReceived = 0,
                    TotalFlagsReceived = 0,
                    ActivityByMonth = new Dictionary<string, int>(),
                    RecentActions = new List<string>()
                };

                // Safely get each statistic with try-catch
                try
                {
                    if (_viewTrackingService != null)
                    {
                        stats.TotalKDomViews = await _viewTrackingService.GetUserTotalViewsAsync(userId);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Failed to get view stats: {ex.Message}");
                    stats.TotalKDomViews = 0;
                }

                try
                {
                    stats.TotalKDomEdits = await _kdomRepository.GetUserKDomEditsCountAsync(userId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Failed to get edit stats: {ex.Message}");
                    stats.TotalKDomEdits = 0;
                }

                try
                {
                    stats.TotalLikesReceived = await GetUserLikesReceivedAsync(userId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Failed to get likes received: {ex.Message}");
                    stats.TotalLikesReceived = 0;
                }

                try
                {
                    stats.TotalLikesGiven = await GetUserLikesGivenAsync(userId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Failed to get likes given: {ex.Message}");
                    stats.TotalLikesGiven = 0;
                }

                try
                {
                    stats.TotalCommentsReceived = await GetUserCommentsReceivedAsync(userId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Failed to get comments received: {ex.Message}");
                    stats.TotalCommentsReceived = 0;
                }

                try
                {
                    stats.TotalFlagsReceived = await _userRepository.GetUserFlagsReceivedAsync(userId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Failed to get flags received: {ex.Message}");
                    stats.TotalFlagsReceived = 0;
                }

                try
                {
                    var activityByMonth = await _userRepository.GetUserActivityByMonthAsync(userId);
                    stats.ActivityByMonth = activityByMonth ?? new Dictionary<string, int>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Failed to get activity by month: {ex.Message}");
                    stats.ActivityByMonth = new Dictionary<string, int>();
                }

                try
                {
                    var recentActions = await _userRepository.GetUserRecentActionsAsync(userId);
                    stats.RecentActions = recentActions ?? new List<string>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Failed to get recent actions: {ex.Message}");
                    stats.RecentActions = new List<string>();
                }

                Console.WriteLine($"[DEBUG] Successfully built detailed stats for user {userId}");
                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetUserDetailedStatsAsync failed: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                throw;
            }
        }


        public async Task<bool> IsUserAdminAsync(int userId)
        {
            if (userId <= 0) return false;
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.Role == "admin";
        }

        
        public async Task<bool> IsUserAdminOrModeratorAsync(int userId)
        {
            if (userId <= 0) return false;
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.Role == "admin" || user?.Role == "moderator";
        }

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
          
            var userPostIds = await _postRepository.GetUserPostIdsAsync(userId);

            
            var userKDomIds = await _kdomRepository.GetUserKDomIdsAsync(userId, false);

            
            var commentsOnPosts = 0;
            if (userPostIds.Any())
            {
                // Implementăm prin service - contorizăm comentariile pe postările user-ului
                // Aceasta va fi implementată când adăugăm metoda în CommentRepository
                commentsOnPosts = await GetCommentsCountOnPostsAsync(userPostIds, userId);
            }

        
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

        public async Task UpdateProfileAsync(int userId, UserProfileUpdateDto dto)
        {
            try
            {
                Console.WriteLine($"[DEBUG] UpdateProfileAsync called for user {userId}");
                Console.WriteLine($"[DEBUG] DTO: Nickname='{dto.Nickname}', Bio='{dto.Bio}', Theme={dto.ProfileTheme}, AvatarUrl='{dto.AvatarUrl}'");

                // Get user to ensure they exist
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found.");

                // Validate inputs with null safety
                if (!string.IsNullOrEmpty(dto.Nickname) && dto.Nickname.Length > 50)
                    throw new Exception("Nickname cannot exceed 50 characters.");

                if (!string.IsNullOrEmpty(dto.Bio) && dto.Bio.Length > 500)
                    throw new Exception("Bio cannot exceed 500 characters.");

                // FIXED: Only validate URL if it's not null or empty
                if (!string.IsNullOrEmpty(dto.AvatarUrl) && !IsValidUrl(dto.AvatarUrl))
                    throw new Exception("Avatar URL must be a valid URL.");

                // Get existing profile
                var profile = await _profileRepository.GetProfileByUserIdAsync(userId);

                if (profile == null)
                {
                    // Create new profile
                    profile = new UserProfile
                    {
                        UserId = userId,
                        Nickname = dto.Nickname ?? "",
                        Bio = dto.Bio ?? "",
                        ProfileTheme = dto.ProfileTheme,
                        AvatarUrl = dto.AvatarUrl ?? "",
                        JoinedAt = DateTime.UtcNow,
                        RecentlyViewedKDomIds = new List<string>()
                    };

                    await _profileRepository.CreateAsync(profile);
                    Console.WriteLine($"[DEBUG] Created new profile for user {userId}");
                }
                else
                {
                    // Update existing profile with null-safe assignments
                    profile.Nickname = dto.Nickname ?? profile.Nickname ?? "";
                    profile.Bio = dto.Bio ?? profile.Bio ?? "";
                    profile.ProfileTheme = dto.ProfileTheme;
                    profile.AvatarUrl = dto.AvatarUrl ?? profile.AvatarUrl ?? "";

                    await _profileRepository.UpdateAsync(profile);
                    Console.WriteLine($"[DEBUG] Updated existing profile for user {userId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdateProfileAsync failed: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task AddRecentlyViewedKDomAsync(int userId, string kdomId)
        {
            try
            {
                

                if (string.IsNullOrEmpty(kdomId))
                    throw new ArgumentException("K-Dom ID cannot be null or empty.", nameof(kdomId));

                if (userId <= 0)
                    throw new ArgumentException("User ID must be positive.", nameof(userId));

                // Verifică dacă K-Dom-ul există
                var kdom = await _kdomRepository.GetByIdAsync(kdomId);
                if (kdom == null)
                {
                    throw new ArgumentException($"K-Dom with ID {kdomId} not found.", nameof(kdomId));
                }

                // Verifică dacă utilizatorul există
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException($"User with ID {userId} not found.", nameof(userId));
                }

                await _profileRepository.AddRecentlyViewedKDomAsync(userId, kdomId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<string>> GetRecentlyViewedKDomIdsAsync(int userId)
        {
            try
            {

                if (userId <= 0)
                    throw new ArgumentException("User ID must be positive.", nameof(userId));

                var ids = await _profileRepository.GetRecentlyViewedKDomIdsAsync(userId);

                return ids;
            }
            catch (Exception ex)
            {
                throw;
            }
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

        private static bool IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return true; // Empty URLs are valid (will be handled as no avatar)

            return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

    }
}