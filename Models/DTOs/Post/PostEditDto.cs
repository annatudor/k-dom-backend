using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.Post
{
    public class PostEditDto
    {
        [Required]
        public string ContentHtml { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = new();
    }
}
