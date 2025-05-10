using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using KDomBackend.Enums;

namespace KDomBackend.Models.MongoEntities
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public int UserId { get; set; }
        
        [BsonRepresentation(BsonType.String)]
        public NotificationType Type { get; set; }

        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }
}
