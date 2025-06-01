using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.KDom
{
    public class BulkOperationDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one K-Dom ID is required.")]
        public List<string> KDomIds { get; set; } = new();
    }
}