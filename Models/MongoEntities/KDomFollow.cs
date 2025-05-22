using MongoDB.Bson;

namespace KDomBackend.Models.MongoEntities
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    namespace KDomBackend.Models.MongoEntities
    {
        public class KDomFollow
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; } = string.Empty;

            public int UserId { get; set; }
            public string KDomId { get; set; } = string.Empty;

            public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
        }
    }


}
