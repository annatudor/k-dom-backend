using KDomBackend.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomCreateDto
    {
        public string? ParentId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
       
        [BsonRepresentation(BsonType.String)]
        public Language Language { get; set; }
        public string Description { get; set; } = string.Empty;
        
        [BsonRepresentation(BsonType.String)]
        public Hub Hub { get; set; }
        public bool IsForKids { get; set; } = false;
        [BsonRepresentation(BsonType.String)]
        public KDomTheme Theme { get; set; } = KDomTheme.Light;

        public string ContentHtml { get; set; } = string.Empty;
    }

}
