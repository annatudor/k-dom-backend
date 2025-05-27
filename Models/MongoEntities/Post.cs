using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace KDomBackend.Models.MongoEntities
{
    public class Post
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.Int32)]
        public int UserId { get; set; }
        public string ContentHtml { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();

        [BsonRepresentation(BsonType.Int32)]
        public List<int> Likes { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }


    }
}
