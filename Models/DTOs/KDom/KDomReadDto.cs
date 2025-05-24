using KDomBackend.Enums;

namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomReadDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Hub Hub { get; set; }
        public KDomTheme Theme { get; set; } = KDomTheme.Light;
        public string ContentHtml { get; set; } = string.Empty;
        public Language Language { get; set; }
        public bool IsForKids { get; set; }
        public int UserId { get; set; }  
        public string AuthorUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastEditedAt { get;  set; }
    }
}
