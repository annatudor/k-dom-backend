using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KDomBackend.Models.MongoEntities
{
    public class KDomMetadataEdit
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string KDomId { get; set; } = string.Empty;
        public int UserId { get; set; }

        public string PreviousTitle { get; set; } = string.Empty;
        public string PreviousDescription { get; set; } = string.Empty;
        public string PreviousLanguage { get; set; } = string.Empty;
        public string PreviousHub { get; set; } = string.Empty;
        public bool PreviousIsForKids { get; set; }
        public string PreviousTheme { get; set; } = "light";

        public string? EditNote { get; set; }
        public DateTime EditedAt { get; set; } = DateTime.UtcNow;
    }
}
