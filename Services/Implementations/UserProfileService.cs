using KDomBackend.Enums;
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

        public UserProfileService(
            IUserRepository userRepository,
            IUserProfileRepository profileRepository,
            IFollowRepository followRepository)
        {
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _followRepository = followRepository;
        }

        /// <summary>
        /// Obține profilul unui utilizator (public sau privat)
        /// </summary>
        public async Task<UserProfileReadDto> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var profile = await _profileRepository.GetProfileByUserIdAsync(userId);

            var followersCount = await _followRepository.GetFollowersCountAsync(userId);
            var followingCount = await _followRepository.GetFollowingCountAsync(userId);

            return new UserProfileReadDto
            {
                UserId = user.Id,
                Username = user.Username,
                Nickname = profile?.Nickname ?? "",
                AvatarUrl = profile?.AvatarUrl ?? "",
                Bio = profile?.Bio ?? "",
                ProfileTheme = profile?.ProfileTheme ?? ProfileTheme.Default,
                FollowersCount = followersCount,
                FollowingCount = followingCount,
                JoinedAt = user.CreatedAt
            };
        }

        /// <summary>
        /// Actualizează profilul unui utilizator
        /// IMPORTANT: Această metodă nu verifică permisiunile - trebuie verificate în controller
        /// </summary>
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

        /// <summary>
        /// Adaugă un K-Dom în lista celor vizionate recent
        /// </summary>
        public async Task AddRecentlyViewedKDomAsync(int userId, string kdomId)
        {
            if (string.IsNullOrEmpty(kdomId))
                throw new ArgumentException("K-Dom ID cannot be null or empty.");

            await _profileRepository.AddRecentlyViewedKDomAsync(userId, kdomId);
        }

        /// <summary>
        /// Obține lista K-Dom-urilor vizionate recent
        /// </summary>
        public async Task<List<string>> GetRecentlyViewedKDomIdsAsync(int userId)
        {
            return await _profileRepository.GetRecentlyViewedKDomIdsAsync(userId);
        }

        /// <summary>
        /// Verifică dacă un utilizator poate actualiza profilul altui utilizator
        /// </summary>
        public async Task<bool> CanUserUpdateProfileAsync(int currentUserId, int targetUserId)
        {
            // Un utilizator își poate actualiza propriul profil
            if (currentUserId == targetUserId)
                return true;

            // Verifică dacă utilizatorul curent este admin
            var currentUser = await _userRepository.GetByIdAsync(currentUserId);
            return currentUser?.Role == "admin";
        }

        /// <summary>
        /// Metodă helper pentru validarea permisiunilor înainte de actualizare
        /// </summary>
        public async Task ValidateUpdatePermissionsAsync(int currentUserId, int targetUserId)
        {
            if (!await CanUserUpdateProfileAsync(currentUserId, targetUserId))
            {
                throw new UnauthorizedAccessException("You don't have permission to update this profile.");
            }
        }
    }
}