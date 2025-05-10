using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KDomBackend.Models.MongoEntities
{
    public class KDom
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public string Description { get; set; } = string.Empty; 
        public string Hub { get; set; } = string.Empty;
        public bool IsForKids { get; set; } = false;
        public string Theme { get; set; } = "light";
        public string Status { get; set; } = "pending";
        public string ContentHtml { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastEditedtAt { get; set; }
    }
}
