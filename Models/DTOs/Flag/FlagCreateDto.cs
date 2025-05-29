using System.ComponentModel.DataAnnotations;
using KDomBackend.Enums;
using Spryer;

namespace KDomBackend.Models.DTOs.Flag
{
    public class FlagCreateDto
    {
        [Required]
        public DbEnum<ContentType> ContentType { get; set; }

        [Required]
        public string ContentId { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }
}
