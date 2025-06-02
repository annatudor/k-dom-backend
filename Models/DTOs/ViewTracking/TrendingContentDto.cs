using KDomBackend.Enums;

namespace KDomBackend.Models.DTOs.ViewTracking
{
    public class TrendingContentDto
    {
        public string ContentId { get; set; } = string.Empty;
        public ContentType ContentType { get; set; }
        public string ContentTitle { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int RecentViews { get; set; }
        public double TrendingScore { get; set; }
        public DateTime LastViewed { get; set; }
    }
}