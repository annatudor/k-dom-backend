using KDomBackend.Data;
using KDomBackend.Helpers;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Repositories.Implementations;
using KDomBackend.Services.Interfaces;
using BCrypt.Net;

namespace KDomBackend.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly IPasswordResetRepository _passwordResetRepository;

        public UserService(IUserRepository userRepository, JwtHelper jwtHelper, IPasswordResetRepository passwordResetRepository)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _passwordResetRepository = passwordResetRepository;
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
            return userId;
        }

        public async Task<string> AuthenticateAsync(UserLoginDto dto)
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

            return token;
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
            await _userRepository.UpdatePasswordAsync(user.Id, newHashed);
            await _passwordResetRepository.MarkAsUsedAsync(tokenRecord.Id);
        }


    }
}
