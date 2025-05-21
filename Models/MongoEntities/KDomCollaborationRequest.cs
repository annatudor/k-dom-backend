using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using KDomBackend.Enums;

public class KDomCollaborationRequest
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string KDomId { get; set; } = string.Empty;
    public int UserId { get; set; }
    [BsonRepresentation(BsonType.String)]
    public CollaborationRequestStatus Status { get; set; } = CollaborationRequestStatus.Pending;
    public string? Message { get; set; } = null;
    public string? RejectionReason { get; set; } = null;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedBy { get; set; }
}
