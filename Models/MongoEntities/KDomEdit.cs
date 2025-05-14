using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class KDomEdit
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string KDomId { get; set; } = string.Empty;
    public int UserId { get; set; }

    public string PreviousContentHtml { get; set; } = string.Empty;
    public string NewContentHtml { get; set; } = string.Empty;

    public string? EditNote { get; set; }
    public bool IsMinor { get; set; } = false;
    public bool IsAutoSave { get; set; } = false;

    public DateTime EditedAt { get; set; } = DateTime.UtcNow;
}
