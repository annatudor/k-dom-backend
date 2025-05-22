using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.Post
{
    public class PostCreateDto
    {
        [Required]
        public string ContentHtml { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = new();
        public string? KDomId { get; set; }

    }
}
