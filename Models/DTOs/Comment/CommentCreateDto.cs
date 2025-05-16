using System.ComponentModel.DataAnnotations;
using KDomBackend.Enums;

namespace KDomBackend.Models.DTOs.Comment
{
    public class CommentCreateDto
    {
        [Required]
        public CommentTargetType TargetType { get; set; }

        [Required]
        public string TargetId { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public string Text { get; set; } = string.Empty;

        public string? ParentCommentId { get; set; }
    }
}
