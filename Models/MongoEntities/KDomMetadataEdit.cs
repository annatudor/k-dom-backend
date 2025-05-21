using KDomBackend.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KDomBackend.Models.MongoEntities
{
    public class KDomMetadataEdit
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty; 
        public string? ParentId { get; set; }

        public string KDomId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string? PreviousParentId { get; set; }

        public string PreviousTitle { get; set; } = string.Empty;
        public string PreviousDescription { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        public Language PreviousLanguage { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Hub PreviousHub { get; set; } 

        public bool PreviousIsForKids { get; set; }
        public string PreviousTheme { get; set; } = "light";

        public string? EditNote { get; set; }
        public DateTime EditedAt { get; set; } = DateTime.UtcNow;
    }
}
