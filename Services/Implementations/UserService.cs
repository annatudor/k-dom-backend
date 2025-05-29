
using KDomBackend.Models.Entities;
using KDomBackend.Repositories.Interfaces;

using KDomBackend.Services.Interfaces;


namespace KDomBackend.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
      
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            
        }

        public async Task<string> GetUsernameByUserIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.Username ?? "unknown";
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }
    }
}
