namespace KDomBackend.Models.DTOs.ViewTracking
{
    public class ViewTrendsDto
    {
        public double GrowthRate { get; set; } // Percentage
        public int MostActiveHour { get; set; } // 0-23
        public string MostActiveDay { get; set; } = string.Empty; // Monday, Tuesday, etc.
        public int PeakViews { get; set; }
        public DateTime PeakViewsDate { get; set; }
        public Dictionary<string, double> ViewDistribution { get; set; } = new(); // Hour distribution
    }
}