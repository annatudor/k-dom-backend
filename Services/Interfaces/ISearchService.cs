using KDomBackend.Models.DTOs.Search;

namespace KDomBackend.Services.Interfaces
{
    public interface ISearchService
    {

        Task<GlobalSearchResultDto> GlobalSearchAsync(string query);

    }
}
