using KDomBackend.Enums;

namespace KDomBackend.Models.DTOs.Collaboration
{
    public class CollaborationRequestReadDto
    {
        public string Id { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string? Username { get; set; }
        public CollaborationRequestStatus Status { get; set; }
        public string? Message { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
