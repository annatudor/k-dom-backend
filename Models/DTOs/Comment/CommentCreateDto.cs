namespace KDomBackend.Models.DTOs.Comment
{
        public class CommentCreateDto
        {
            public string TargetId { get; set; } = string.Empty;
            public string TargetType { get; set; } = "post"; // sau "kdom"
            public string Text { get; set; } = string.Empty;
            public string? ParentCommentId { get; set; } // null daca e root
        }

}
