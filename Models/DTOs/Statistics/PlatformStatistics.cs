// Models/DTOs/Statistics/PlatformStatsDto.cs
namespace KDomBackend.Models.DTOs.Statistics
{
    public class PlatformStatsDto
    {
        public int TotalKDoms { get; set; }
        public int TotalCategories { get; set; }
        public int ActiveCollaborators { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
    }

    public class CategoryStatsDto
    {
        public string Hub { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<FeaturedKDomDto> Featured { get; set; } = new();
    }

    public class FeaturedKDomDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int FollowersCount { get; set; }
    }

    public class FeaturedKDomForHomepageDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Hub { get; set; } = string.Empty;
        public int FollowersCount { get; set; }
        public int PostsCount { get; set; }
        public int CommentsCount { get; set; }
    }

    public class HomepageDataDto
    {
        public PlatformStatsDto PlatformStats { get; set; } = new();
        public List<CategoryStatsDto> CategoryStats { get; set; } = new();
        public List<FeaturedKDomForHomepageDto> FeaturedKDoms { get; set; } = new();
    }
}