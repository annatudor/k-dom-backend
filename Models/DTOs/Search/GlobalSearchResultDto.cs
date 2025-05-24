using KDomBackend.Models.DTOs.KDom;

namespace KDomBackend.Models.DTOs.Search;

public class GlobalSearchResultDto
{
    public List<KDomTagSearchResultDto> Kdoms { get; set; } = new();
    public List<UserSearchDto> Users { get; set; } = new();
    public List<string> Tags { get; set; } = new(); 
}
