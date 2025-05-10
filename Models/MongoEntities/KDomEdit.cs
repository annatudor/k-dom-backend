using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace KDomBackend.Models.MongoEntities
{
    public class KDomEdit
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string KDomId { get; set; } = string.Empty;
        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Hub { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public bool IsForKids { get; set; }
        public string Theme { get; set; } = "light";
        public string ContentHtml { get; set; } = string.Empty;

        public string? EditNote { get; set; } 
        public bool IsMinor { get; set; } = false;

        public DateTime EditedAt { get; set; } = DateTime.UtcNow;
    }
}
