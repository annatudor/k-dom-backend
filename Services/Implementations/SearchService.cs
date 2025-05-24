using KDomBackend.Models.DTOs.KDom;
using KDomBackend.Models.DTOs.Search;
using KDomBackend.Repositories.Interfaces;
using KDomBackend.Services.Interfaces;

namespace KDomBackend.Services.Implementations
{
    public class SearchService : ISearchService
    {
        private readonly IKDomRepository _kdomRepository;
        private readonly IUserRepository _userRepository;

        public SearchService(IKDomRepository kdomRepository, IUserRepository userRepository)
        {
            _kdomRepository = kdomRepository;
            _userRepository = userRepository;
        }

        public async Task<GlobalSearchResultDto> GlobalSearchAsync(string query)
        {
            var result = new GlobalSearchResultDto();

            var kdoms = await _kdomRepository.SearchByQueryAsync(query);
            result.Kdoms = kdoms.Select(k => new KDomTagSearchResultDto
            {
                Id = k.Id,
                Title = k.Title,
                Slug = k.Slug
            }).ToList();

            var users = await _userRepository.SearchUsersAsync(query);
            result.Users = users.Select(u => new UserSearchDto
            {
                Id = u.Id,
                Username = u.Username
            }).ToList();

            
            result.Tags = result.Kdoms.Select(k => k.Slug).ToList();

            return result;
        }
    }

}
