using KDomBackend.Enums;

public class CommentReadDto
{
    public string Id { get; set; } = string.Empty;
    public CommentTargetType TargetType { get; set; }
    public string TargetId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty; 
    public string Text { get; set; } = string.Empty;
    public string? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }

    public List<int> Likes { get; set; } = new();
    public int LikeCount { get; set; }
    public bool IsLikedByUser { get; set; } = false;
}
