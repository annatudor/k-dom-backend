using KDomBackend.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomMetadataEditReadDto
    {
        public string Id { get; set; } = string.Empty;
        public string? PreviousParentId { get; set; }
        public string PreviousTitle { get; set; } = string.Empty;
        public string PreviousDescription { get; set; } = string.Empty;
        [BsonRepresentation(BsonType.String)]
        public Language PreviousLanguage { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Hub PreviousHub { get; set; }
        public bool PreviousIsForKids { get; set; }
        [BsonRepresentation(BsonType.String)]
        public KDomTheme PreviousTheme { get; set; } = KDomTheme.Light;
        public DateTime EditedAt { get; set; }
    }
}
