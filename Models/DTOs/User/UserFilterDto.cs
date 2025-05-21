using KDomBackend.Models.DTOs.Common;

namespace KDomBackend.Models.DTOs.User
{
    public class UserFilterDto : PagedFilterDto
    {
        public string? Role { get; set; }
        public string? Search { get; set; }
    }
}
