using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomEditReadDto
    {
        [Required]
        public string KDomSlug { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string EditNote { get; set; } = string.Empty;
        public bool IsMinor { get; set; }
        public DateTime EditedAt { get; set; }
    }
}
