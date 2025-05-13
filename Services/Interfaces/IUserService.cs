using KDomBackend.Models.DTOs.User;

namespace KDomBackend.Services.Interfaces
{
    public interface IUserService
    {
        Task<int> RegisterUserAsync(UserRegisterDto dto);
        Task<string> AuthenticateAsync(UserLoginDto dto);
        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task RequestPasswordResetAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);


    }
}
