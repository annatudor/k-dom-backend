using KDomBackend.Enums;

namespace KDomBackend.Models.DTOs.ViewTracking
{
    public class ViewStatsDto
    {
        public ContentType ContentType { get; set; }
        public string ContentId { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int RecentViews { get; set; } // Last 24h
        public int UniqueViewers { get; set; }
        public DateTime? LastViewed { get; set; }
        public double GrowthRate { get; set; } // Percentage growth
        public string PopularityLevel { get; set; } = "New"; // New, Growing, Popular, Viral
    }
}