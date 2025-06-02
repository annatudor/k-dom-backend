namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomDiscussionStatsDto
    {
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public int UniquePosterCount { get; set; }
        public DateTime? LastPostDate { get; set; }
        public DateTime? FirstPostDate { get; set; }
    }
}
