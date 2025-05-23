﻿using KDomBackend.Data;
using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Repositories.Implementations;
using KDomBackend.Services.Interfaces;
using BCrypt.Net;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Common;
using MongoDB.Driver;

namespace KDomBackend.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly IUserProfileRepository _profileRepository;
        private readonly IFollowRepository _followRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly MongoDbContext _mongoDbContext;


        public UserService(
            IUserRepository userRepository, 
            JwtHelper jwtHelper,
            IPasswordResetRepository passwordResetRepository,
            IUserProfileRepository profileRepository,
            IFollowRepository followRepository, IAuditLogRepository auditLogRepository,
            MongoDbContext mongoDbContext
            )
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _passwordResetRepository = passwordResetRepository;
            _profileRepository = profileRepository;
            _followRepository = followRepository;
            _auditLogRepository = auditLogRepository;
            _mongoDbContext = mongoDbContext;
        }


        public async Task<int> RegisterUserAsync(UserRegisterDto dto)
        {
            if (await _userRepository.ExistsByEmailAsync(dto.Email))
                throw new Exception("Email already in use.");

            if (await _userRepository.ExistsByUsernameAsync(dto.Username))
                throw new Exception("Username already taken.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                RoleId = 1,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var userId = await _userRepository.CreateAsync(user);
           
            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = userId,
                Action = AuditAction.CreateUser,
                TargetType = AuditTargetType.User,
                TargetId = userId.ToString(),
                Details = "Account created.",
                CreatedAt = DateTime.UtcNow
            });

            return userId;
        }

            public async Task<string> AuthenticateAsync(UserLoginDto dto)
        {
            try
            {
                User? user = null;

                if (dto.Identifier.Contains("@"))
                    user = await _userRepository.GetByEmailAsync(dto.Identifier);
                else
                    user = await _userRepository.GetByUsernameAsync(dto.Identifier);

                if (user == null || !user.IsActive)
                    throw new Exception("User not found or inactive.");

                var passwordMatch = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
                if (!passwordMatch)
                    throw new Exception("Invalid password.");

                var token = _jwtHelper.GenerateToken(user);

                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    UserId = user.Id,
                    Action = AuditAction.LoginSuccess,
                    TargetType = AuditTargetType.User,
                    TargetId = user.Id.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    Details = "Login successful."
                });

                return token;
            }
            catch (Exception ex)
            {
                
                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    UserId = 0,
                    Action = AuditAction.LoginFailed,
                    TargetType = AuditTargetType.User,
                    TargetId = dto.Identifier,
                    CreatedAt = DateTime.UtcNow,
                    Details = $"Login failed: {ex.Message}"
                });

                throw;
            }
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
                throw new Exception("User not found or inactive.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new Exception("Current password is incorrect.");

            var newHashed = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordHash = newHashed;

            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = user.Id,
                Action = AuditAction.ChangePassword,
                TargetType = AuditTargetType.User,
                TargetId = user.Id.ToString(),
                CreatedAt = DateTime.UtcNow,
                Details = "Password changed."
            });

            await _userRepository.UpdatePasswordAsync(user.Id, newHashed);
        }

        public async Task RequestPasswordResetAsync(ForgotPasswordDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || !user.IsActive)
                throw new Exception("No user found with that email.");

            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30), 
                CreatedAt = DateTime.UtcNow
            };

            await _passwordResetRepository.CreateAsync(resetToken);

            Console.WriteLine($"[INFO] Password reset link: https://kdom/reset?token={token}");
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var tokenRecord = await _passwordResetRepository.GetByTokenAsync(dto.Token);

            if (tokenRecord == null || tokenRecord.Used || tokenRecord.ExpiresAt < DateTime.UtcNow)
                throw new Exception("Invalid or expired token.");

            var user = await _userRepository.GetByIdAsync(tokenRecord.UserId);
            if (user == null || !user.IsActive)
                throw new Exception("User not found.");

            var newHashed = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = user.Id,
                Action = AuditAction.ResetPassword,
                TargetType = AuditTargetType.User,
                TargetId = user.Id.ToString(),
                CreatedAt = DateTime.UtcNow,
                Details = "Password reset."
            });

            await _userRepository.UpdatePasswordAsync(user.Id, newHashed);
            await _passwordResetRepository.MarkAsUsedAsync(tokenRecord.Id);
        }
        public async Task<string> GetUsernameByUserIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.Username ?? "unknown";
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

        public async Task ChangeUserRoleAsync(int targetUserId, string newRole, int adminUserId)
        {
            var validRoles = new[] { "user", "moderator", "admin" };
            if (!validRoles.Contains(newRole.ToLower()))
                throw new ArgumentException("Invalid role.");

            var targetUser = await _userRepository.GetByIdAsync(targetUserId);
            if (targetUser == null)
                throw new Exception("User does not exist.");

            await _userRepository.UpdateRoleAsync(targetUserId, newRole.ToLower());

            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = adminUserId,
                Action = AuditAction.ChangeRole,
                TargetType = AuditTargetType.User,
                TargetId = targetUserId.ToString(),
                CreatedAt = DateTime.UtcNow,
                Details = $"Role changed in '{newRole}'"
            });

        }

        public async Task<PagedResult<UserPublicDto>> GetAllPaginatedAsync(UserFilterDto filter)
        {
            var totalCount = await _userRepository.CountAsync(filter.Role, filter.Search);
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
            var skip = (filter.Page - 1) * filter.PageSize;

            var users = await _userRepository.GetPaginatedAsync(skip, filter.PageSize, filter.Role, filter.Search);

            return new PagedResult<UserPublicDto>
            {
                TotalCount = totalCount,
                CurrentPage = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                Items = users.Select(u => new UserPublicDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role ?? "",
                    CreatedAt = u.CreatedAt
                }).ToList()
            };
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task AddRecentlyViewedKDomAsync(int userId, string kdomId)
        {
            var filter = Builders<UserProfile>.Filter.Eq(p => p.UserId, userId);
            var profile = await _mongoDbContext.UserProfiles.Find(filter).FirstOrDefaultAsync();

            if (profile == null) return;

            profile.RecentlyViewedKDomIds.Remove(kdomId);
            profile.RecentlyViewedKDomIds.Insert(0, kdomId);

            if (profile.RecentlyViewedKDomIds.Count > 3)
                profile.RecentlyViewedKDomIds = profile.RecentlyViewedKDomIds.Take(3).ToList();

            await _mongoDbContext.UserProfiles.ReplaceOneAsync(filter, profile);
        }

        public async Task<List<string>> GetRecentlyViewedKDomIdsAsync(int userId)
        {
            var profile = await _mongoDbContext.UserProfiles
                .Find(p => p.UserId == userId)
                .FirstOrDefaultAsync();

            return profile?.RecentlyViewedKDomIds ?? new();
        }


    }
}
