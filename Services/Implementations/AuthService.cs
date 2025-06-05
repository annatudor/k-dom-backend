using KDomBackend.Data;
using KDomBackend.Enums;
using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;


namespace KDomBackend.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public AuthService(
            IUserRepository userRepository,
            JwtHelper jwtHelper,
            IPasswordResetRepository passwordResetRepository,
            IUserProfileRepository profileRepository,
            IAuditLogRepository auditLogRepository
            )
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _passwordResetRepository = passwordResetRepository;
            _auditLogRepository = auditLogRepository;
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
                Console.WriteLine($"[DEBUG] AuthService - Login attempt for: {dto.Identifier}");

                User? user = null;
                if (dto.Identifier.Contains("@"))
                {
                    Console.WriteLine($"[DEBUG] AuthService - Searching by email");
                    user = await _userRepository.GetByEmailAsync(dto.Identifier);
                }
                else
                {
                    Console.WriteLine($"[DEBUG] AuthService - Searching by username");
                    user = await _userRepository.GetByUsernameAsync(dto.Identifier);
                }

                if (user == null || !user.IsActive)
                {
                    Console.WriteLine($"[DEBUG] AuthService - User not found or inactive");
                    throw new Exception("User not found or inactive.");
                }

                Console.WriteLine($"[DEBUG] AuthService - User found: {user.Username}");
                Console.WriteLine($"[DEBUG] AuthService - User Role: {user.Role}");
                Console.WriteLine($"[DEBUG] AuthService - User RoleId: {user.RoleId}");

                var passwordMatch = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
                if (!passwordMatch)
                {
                    Console.WriteLine($"[DEBUG] AuthService - Password verification failed");
                    throw new Exception("Invalid password.");
                }

                Console.WriteLine($"[DEBUG] AuthService - Password verified successfully");

                // FIX: Adaugă DateTime.UtcNow ca al doilea parametru
                var loginTime = DateTime.UtcNow;
                await _userRepository.UpdateLastLoginAsync(user.Id, loginTime);

                Console.WriteLine($"[DEBUG] AuthService - Updated last login time");

                // Generate token
                Console.WriteLine($"[DEBUG] AuthService - Generating token for user: {user.Username}, Role: {user.Role}");
                var token = _jwtHelper.GenerateToken(user);

                Console.WriteLine($"[DEBUG] AuthService - Token generated successfully");

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
                Console.WriteLine($"[DEBUG] AuthService - Login failed: {ex.Message}");

                await _auditLogRepository.CreateAsync(new AuditLog
                {
                    UserId = null,
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
            Console.WriteLine($"[DEBUG] ChangePasswordAsync called for user ID: {userId}");

            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "ChangePasswordDto cannot be null");

            var user = await _userRepository.GetByIdAsync(userId);
            Console.WriteLine($"[DEBUG] User found: {user != null}");
            Console.WriteLine($"[DEBUG] User ID: {user?.Id}");

            if (user == null || !user.IsActive)
                throw new Exception("User not found or inactive.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new Exception("New password cannot be empty.");

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                throw new Exception("Current password cannot be empty.");

            // Verifică dacă utilizatorul are un password hash valid
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                Console.WriteLine($"[ERROR] User {userId} has empty or null PasswordHash!");
                throw new Exception("User account has invalid password data. Please contact administrator or reset your password.");
            }

            Console.WriteLine($"[DEBUG] About to verify password");
            Console.WriteLine($"[DEBUG] Current password from DTO: '{dto.CurrentPassword}'");
            Console.WriteLine($"[DEBUG] Stored password hash: '{user.PasswordHash}'");

            try
            {
                var passwordMatch = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
                Console.WriteLine($"[DEBUG] Password verification result: {passwordMatch}");

                if (!passwordMatch)
                    throw new Exception("Current password is incorrect.");
            }
            catch (Exception ex) when (ex.Message.Contains("Value cannot be null"))
            {
                Console.WriteLine($"[ERROR] BCrypt.Verify failed due to null parameter: {ex.Message}");
                throw new Exception("Password verification failed due to invalid stored password data.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] BCrypt.Verify threw unexpected exception: {ex.Message}");
                throw new Exception($"Password verification failed: {ex.Message}");
            }

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
            Console.WriteLine($"[DEBUG] Password updated successfully for user {user.Id}");
        }

        public async Task RequestPasswordResetAsync(ForgotPasswordDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || !user.IsActive)
                throw new Exception("No user found with that email.");

            var token = Guid.NewGuid().ToString("N");


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
            Console.WriteLine($"[DEBUG] ExpiresAt: {tokenRecord.ExpiresAt}, Now: {DateTime.UtcNow}");
            Console.WriteLine($"[DEBUG] Row: ID={tokenRecord?.Id}, ExpiresAt={tokenRecord?.ExpiresAt}");

            if (tokenRecord == null || tokenRecord.Used || tokenRecord.ExpiresAt < DateTime.UtcNow)
                throw new Exception("Invalid or expired token.");

            var user = await _userRepository.GetByIdAsync(tokenRecord.UserId);
            if (user == null || !user.IsActive)
                throw new Exception("User not found.");

            var newHashed = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            Console.WriteLine($"[DEBUG] Incoming token: {dto.Token}");

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
            Console.WriteLine($"[DEBUG] Mark token ID {tokenRecord.Id} as used.");
            await _passwordResetRepository.MarkAsUsedAsync(tokenRecord.Id);
        }


    }
}
