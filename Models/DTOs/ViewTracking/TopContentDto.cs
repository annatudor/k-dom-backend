using KDomBackend.Enums;

namespace KDomBackend.Models.DTOs.ViewTracking
{
    public class TopContentDto
    {
        public string ContentId { get; set; } = string.Empty;
        public ContentType ContentType { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Slug { get; set; }
    }
}