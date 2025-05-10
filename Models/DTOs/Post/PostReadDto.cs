namespace KDomBackend.Models.DTOs.Post
{
    public class PostReadDto
    {
        public string Id { get; set; } = string.Empty;
        public string ContentHtml { get; set; } = string.Empty;
        public string AuthorUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
    }

}
