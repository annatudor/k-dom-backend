namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomStatsDto
    {
        public int ViewsCount { get; set; }
        public int FollowersCount { get; set; }
        public int CommentsCount { get; set; }
        public int EditsCount { get; set; }
        public int CollaboratorsCount { get; set; }
        public DateTime? LastActivity { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalInteractions => ViewsCount + FollowersCount + CommentsCount + EditsCount;
    }
}
