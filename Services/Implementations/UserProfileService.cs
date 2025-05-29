using KDomBackend.Data;
using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;
using MongoDB.Driver;

namespace KDomBackend.Services.Implementations
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserProfileRepository _profileRepository;
        private readonly IFollowRepository _followRepository;

        public UserProfileService(
            IUserRepository userRepository,
            JwtHelper jwtHelper,
            IPasswordResetRepository passwordResetRepository,
            IUserProfileRepository profileRepository,
            IFollowRepository followRepository, 
            IAuditLogRepository auditLogRepository,
            IUserProfileRepository userProfileRepository
            )
        {
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _followRepository = followRepository;

        }

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
                ProfileTheme = profile.ProfileTheme,
                FollowersCount = followersCount,
                FollowingCount = followingCount,
                JoinedAt = user.CreatedAt
            };
        }

        public async Task UpdateProfileAsync(int userId, UserProfileUpdateDto dto)
        {
            var profile = await _profileRepository.GetProfileByUserIdAsync(userId);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    Nickname = dto.Nickname,
                    Bio = dto.Bio,
                    ProfileTheme = dto.ProfileTheme,
                    AvatarUrl = dto.AvatarUrl,
                    JoinedAt = DateTime.UtcNow
                };

                await _profileRepository.CreateAsync(profile);
            }
            else
            {
                profile.Nickname = dto.Nickname;
                profile.Bio = dto.Bio;
                profile.ProfileTheme = dto.ProfileTheme;
                profile.AvatarUrl = dto.AvatarUrl;

                await _profileRepository.UpdateAsync(profile);
            }
        }

        public async Task AddRecentlyViewedKDomAsync(int userId, string kdomId)
        {
            await _profileRepository.AddRecentlyViewedKDomAsync(userId, kdomId);
        }


        public async Task<List<string>> GetRecentlyViewedKDomIdsAsync(int userId)
        {
            return await _profileRepository.GetRecentlyViewedKDomIdsAsync(userId);
        }


    }
}
