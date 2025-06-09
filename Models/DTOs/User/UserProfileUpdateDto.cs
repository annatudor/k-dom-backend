using KDomBackend.Enums;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.User
{
    public class UserProfileUpdateDto
    {
        [MaxLength(50)]
        public string Nickname { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Bio { get; set; } = string.Empty;

        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public ProfileTheme ProfileTheme { get; set; } = ProfileTheme.Default;

        public string AvatarUrl { get; set; } = string.Empty;
    }
}
