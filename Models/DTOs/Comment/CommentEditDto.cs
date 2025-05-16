using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.Comment
{
    public class CommentEditDto
    {
        [Required]
        public string Text { get; set; } = string.Empty;
    }
}
