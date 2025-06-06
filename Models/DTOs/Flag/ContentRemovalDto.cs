using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.Flag
{
    public class ContentRemovalDto
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
