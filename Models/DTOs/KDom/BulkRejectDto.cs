using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.KDom
{
    public class BulkRejectDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one K-Dom ID is required.")]
        public List<string> KDomIds { get; set; } = new();

        [Required]
        [MinLength(10, ErrorMessage = "Rejection reason must be at least 10 characters long.")]
        [MaxLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters.")]
        public string Reason { get; set; } = string.Empty;
    }
}