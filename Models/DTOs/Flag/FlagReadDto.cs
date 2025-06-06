// Models/DTOs/Flag/FlagReadDto.cs - Enhanced version
using KDomBackend.Enums;
using Spryer;

namespace KDomBackend.Models.DTOs.Flag
{
    public class FlagReadDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ReporterUsername { get; set; } = string.Empty; // Cine a raportat
        public DbEnum<ContentType> ContentType { get; set; }
        public string ContentId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }

        public FlaggedContentDto? Content { get; set; }
        public bool ContentExists { get; set; } = true; // Dacă conținutul există încă
    }

    public class FlaggedContentDto
    {
        public string AuthorUsername { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public string Title { get; set; } = string.Empty; // Pentru K-Dom
        public string Text { get; set; } = string.Empty;   // Pentru Post/Comment
        public DateTime CreatedAt { get; set; }
        public string? ParentInfo { get; set; } = null;    // Pentru Comments - pe ce post/kdom
        public List<string> Tags { get; set; } = new();    // Pentru Posts
    }
}