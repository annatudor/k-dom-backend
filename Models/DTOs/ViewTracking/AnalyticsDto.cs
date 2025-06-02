namespace KDomBackend.Models.DTOs.ViewTracking
{
    public class AnalyticsDto
    {
        public int PeriodDays { get; set; }
        public int TotalViews { get; set; }
        public Dictionary<string, int> ViewsByType { get; set; } = new();
        public List<TopContentDto> TopKDoms { get; set; } = new();
        public List<TopContentDto> TopPosts { get; set; } = new();
        public Dictionary<string, int> DailyViews { get; set; } = new();
        public ViewTrendsDto Trends { get; set; } = new();
    }
}