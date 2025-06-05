using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.Moderation
{
    public class RejectAndDeleteDto
    {
        [Required]
        [MinLength(10, ErrorMessage = "Reason must be at least 10 characters long.")]
        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Reason { get; set; } = string.Empty;
    }
}
