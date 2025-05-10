namespace KDomBackend.Models.DTOs.Comment
{
    public class CommentReadDto
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string AuthorUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
    }

}
