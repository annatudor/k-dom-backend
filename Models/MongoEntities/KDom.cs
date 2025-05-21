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
        public string Theme { get; set; } = "light";
        public string Status { get; set; } = "pending";
        public string ContentHtml { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastEditedtAt { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = false;
        public bool IsRejected { get; set; } = false;
        public string? RejectionReason { get; set; }

        public List<int> Collaborators { get; set; } = new();



    }
}
