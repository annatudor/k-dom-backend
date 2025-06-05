using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using KDomBackend.Enums;

namespace KDomBackend.Models.MongoEntities
{
    public class KDom
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string? ParentId { get; set; } = null;


        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        
        [BsonRepresentation(BsonType.String)]
        public Language Language { get; set; }

        public string Description { get; set; } = string.Empty;
        
        [BsonRepresentation(BsonType.String)]
        public Hub Hub { get; set; } 

        public bool IsForKids { get; set; } = false;
        [BsonRepresentation(BsonType.String)]
        public KDomTheme Theme { get; set; } = KDomTheme.Light;
        public string Status { get; set; } = "Pending";
        public string ContentHtml { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastEditedtAt { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = false;
        public bool IsRejected { get; set; } = false;
        public string? RejectionReason { get; set; }
        public DateTime? ModeratedAt { get; set; }
        public int? ModeratedByUserId { get; set; }

        public List<int> Collaborators { get; set; } = new();

        [BsonIgnore]
        public string ModerationStatus
        {
            get
            {
                if (IsApproved) return "Approved";
                if (IsRejected) return "Rejected";
                return "Pending";
            }
        }

        [BsonIgnore]
        public bool IsPending => !IsApproved && !IsRejected;

        [BsonIgnore]
        public bool IsModerated => IsApproved || IsRejected;

    }
}
