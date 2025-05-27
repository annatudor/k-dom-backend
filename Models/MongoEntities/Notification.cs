using KDomBackend.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Int32)]
    public int UserId { get; set; } // cui i se adreseaza

    [BsonRepresentation(BsonType.String)]
    public NotificationType Type { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonRepresentation(BsonType.Int32)]
    public int? TriggeredByUserId { get; set; } // cine a declansat notificarea 

    [BsonRepresentation(BsonType.String)]
    public ContentType? TargetType { get; set; }

    public string? TargetId { get; set; } = null;
}
