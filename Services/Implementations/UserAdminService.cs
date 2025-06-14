﻿using KDomBackend.Data;
using KDomBackend.Enums;
using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.Common;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.Entities;
using KDomBackend.Models.MongoEntities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;
using MongoDB.Driver;

namespace KDomBackend.Services.Implementations
{
    public class UserAdminService : IUserAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public UserAdminService(
            IUserRepository userRepository,
            JwtHelper jwtHelper,
            IPasswordResetRepository passwordResetRepository,
            IUserProfileRepository profileRepository,
            IFollowRepository followRepository, IAuditLogRepository auditLogRepository,
            MongoDbContext mongoDbContext
            )
        {
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
          
        }

        public async Task ChangeUserRoleAsync(int targetUserId, string newRole, int adminUserId)
        {
            var validRoles = new[] { "user", "moderator", "admin" };
            if (!validRoles.Contains(newRole.ToLower()))
                throw new ArgumentException("Invalid role.");

            var targetUser = await _userRepository.GetByIdAsync(targetUserId);
            if (targetUser == null)
                throw new Exception("User does not exist.");

            if (targetUserId == adminUserId)
                throw new Exception("You cannot change your own role.");

            await _userRepository.UpdateRoleAsync(targetUserId, newRole.ToLower());

            await _auditLogRepository.CreateAsync(new AuditLog
            {
                UserId = adminUserId,
                Action = AuditAction.ChangeRole,
                TargetType = AuditTargetType.User,
                TargetId = targetUserId.ToString(),
                CreatedAt = DateTime.UtcNow,
                Details = $"Role changed to '{newRole}'"
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


        // În Services/Implementations/UserAdminService.cs
        public async Task<List<UserPublicDto>> QuickSearchUsersAsync(string query, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                throw new ArgumentException("Query must be at least 2 characters long.");

            if (limit > 50) // Limitare pentru performanță
                limit = 50;

            // Folosește repository-ul existent, dar transformă datele
            var users = await _userRepository.SearchUsersAsync(query);

            // Ia doar numărul limitat și transformă în DTO-ul potrivit
            var limitedUsers = users.Take(limit);

            var result = new List<UserPublicDto>();

            foreach (var user in limitedUsers)
            {
                // Pentru că SearchUsersAsync nu returnează rolul, îl obținem separat
                var fullUser = await _userRepository.GetByIdAsync(user.Id);

                result.Add(new UserPublicDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email ?? "", // Din SearchUsersAsync s-ar putea să nu avem email
                    Role = fullUser?.Role ?? "user",
                    CreatedAt = fullUser?.CreatedAt ?? DateTime.MinValue,
                });
            }

            return result.OrderBy(u => u.Username).ToList();
        }

    }


}
