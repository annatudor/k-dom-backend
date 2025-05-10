namespace KDomBackend.Models.Entities
{
    public class Follow
    {
        public int FollowerId { get; set; } 
        public int FollowingId { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
