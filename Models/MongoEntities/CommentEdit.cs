using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

using KDomBackend.Enums;

namespace KDomBackend.Models.MongoEntities
{
    public class CommentEdit
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string CommentId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.Int32)]
        public int UserId { get; set; }

        public string Text { get; set; } = string.Empty;
        public DateTime EditedAt { get; set; } = DateTime.UtcNow;
      
        [BsonRepresentation(BsonType.String)]
        public CommentTargetType TargetType { get; set; } 
        
        public string TargetId { get; set; } = string.Empty;

    }

}
