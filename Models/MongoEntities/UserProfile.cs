using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KDomBackend.Models.MongoEntities
{
    public class UserProfile
    {
        [BsonId]
        public int UserId { get; set; }

        public string Nickname { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
