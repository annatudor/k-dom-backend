// Models/MongoEntities/ViewTracking.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using KDomBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.MongoEntities
{
    public class ViewTracking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        public ContentType ContentType { get; set; } // Post, KDom

        public string ContentId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.Int32)]
        public int? ViewerId { get; set; } // null pentru guest users

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        // Pentru a evita spam-ul, putem grupa views-urile
        public bool IsUnique { get; set; } = true; // Prima vizualizare într-o sesiune
    }
}

