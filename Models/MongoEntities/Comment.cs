using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using KDomBackend.Enums;

namespace KDomBackend.Models.MongoEntities
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
       
        [BsonRepresentation(BsonType.String)]
        public CommentTargetType TargetType { get; set; }
        
        public string TargetId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Text { get; set; } = string.Empty;
        public List<int> Likes { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }
        public string? ParentCommentId { get; set; } = null;
    }
}
