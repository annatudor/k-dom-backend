using KDomBackend.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KDomBackend.Models.MongoEntities
{
    public class UserProfile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [BsonRepresentation(BsonType.Int32)]
        public int UserId { get; set; }

        public string Nickname { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        [BsonRepresentation(BsonType.String)]
        public ProfileTheme ProfileTheme { get; set; } = ProfileTheme.Default;

        public List<string> RecentlyViewedKDomIds { get; set; } = new();


    }
}
