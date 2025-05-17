namespace KDomBackend.Models.DTOs.Post
{
    public class PostReadDto
    {
        public string Id { get; set; } = string.Empty;
        public string ContentHtml { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
    }
}

