using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using KDomBackend.Enums;

namespace KDomBackend.Models.MongoEntities
{
    public class Activity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public int UserId { get; set; }
        
        [BsonRepresentation(BsonType.String)]
        public ContentType Type { get; set; } 
        
        public string TargetId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
