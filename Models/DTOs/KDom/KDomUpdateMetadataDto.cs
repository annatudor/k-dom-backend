using System.ComponentModel.DataAnnotations;
using KDomBackend.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class KDomUpdateMetadataDto
{
    [Required]
    public string KDomSlug { get; set; } = string.Empty;
    public string KDomId { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;
    public string? ParentId { get; set; }

    public string Description { get; set; } = string.Empty;
    [BsonRepresentation(BsonType.String)]
    public Hub Hub { get; set; }
    [BsonRepresentation(BsonType.String)]
    public Language Language { get; set; } 
    public bool IsForKids { get; set; }
    [BsonRepresentation(BsonType.String)]
    public KDomTheme Theme { get; set; } = KDomTheme.Light;
    public DateTime UpdatedAt { get; set; }
}
