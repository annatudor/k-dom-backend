using KDomBackend.Enums;

namespace KDomBackend.Models.DTOs.Flag
{
    public class FlagReadDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ContentType ContentType { get; set; }
        public string ContentId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}
