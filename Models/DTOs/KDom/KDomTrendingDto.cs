namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomTrendingDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        public int PostScore { get; set; }
        public int CommentScore { get; set; }
        public int FollowScore { get; set; }
        public int EditScore { get; set; }

        public int TotalScore => (PostScore * 3) + (CommentScore * 2) + (FollowScore * 2) + (EditScore * 1);
    }

}
