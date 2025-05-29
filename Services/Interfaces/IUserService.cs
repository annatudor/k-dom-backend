using KDomBackend.Models.DTOs.Common;
using KDomBackend.Models.DTOs.User;
using KDomBackend.Models.Entities;

namespace KDomBackend.Services.Interfaces
{
    public interface IUserService
    {
       
        Task<string> GetUsernameByUserIdAsync(int userId);
        Task<User?> GetUserByUsernameAsync(string username);

    }
}
