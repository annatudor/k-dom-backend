using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class PostEdit
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string PostId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string ContentHtml { get; set; } = string.Empty;
    public DateTime EditedAt { get; set; } = DateTime.UtcNow;
}
