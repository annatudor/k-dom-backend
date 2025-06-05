using KDomBackend.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomReadDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [BsonRepresentation(BsonType.String)]
        public Hub Hub { get; set; }
        [BsonRepresentation(BsonType.String)]
        public KDomTheme Theme { get; set; } = KDomTheme.Light;
        public string ContentHtml { get; set; } = string.Empty;
        [BsonRepresentation(BsonType.String)]
        public Language Language { get; set; }
        public bool IsForKids { get; set; }
        public int UserId { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastEditedAt { get; set; }
        public string? ParentId { get; set; }
        public List<int> Collaborators { get; set; } = new List<int>();

        public bool IsApproved { get; set; } = false;
        public bool IsRejected { get; set; } = false;
        public string? RejectionReason { get; set; }
        public DateTime? ModeratedAt { get; set; }
        public string? ModeratorUsername { get; set; }
        public string Status { get; set; }
        public string ModerationStatus => Status;
        public bool IsPending => Status == "Pending";
        public bool IsModerated => Status == "Approved" || Status == "Rejected";


    }
}
