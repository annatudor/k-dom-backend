namespace KDomBackend.Models.DTOs.ViewTracking
{
    public class UserViewBreakdownDto
    {
        public int TotalViews { get; set; }
        public Dictionary<string, int> Breakdown { get; set; } = new()
        {
            { "Posts", 0 },
            { "KDoms", 0 },
            { "Total", 0 }
        };
        public int UniqueViewers { get; set; }
        public Dictionary<string, int> ViewsByMonth { get; set; } = new();
        public List<TopContentDto> TopContent { get; set; } = new();
    }
}
