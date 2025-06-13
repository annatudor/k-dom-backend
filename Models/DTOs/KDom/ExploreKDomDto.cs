using KDomBackend.Enums;
using KDomBackend.Models.DTOs.Common;

namespace KDomBackend.Models.DTOs.KDom
{
    public class ExploreKDomDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Hub Hub { get; set; }
        public Language Language { get; set; }
        public KDomTheme Theme { get; set; }
        public bool IsForKids { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public int FollowersCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ExploreFilterDto : PagedFilterDto
    {
        public string? Hub { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "newest"; // newest, popular, alphabetical
    }
}