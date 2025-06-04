using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.Collaboration
{
    public class BulkRequestActionDto
    {
        [Required]
        public string KDomId { get; set; } = string.Empty;

        [Required]
        public List<string> RequestIds { get; set; } = new();

        [Required]
        [RegularExpression("^(approve|reject)$", ErrorMessage = "Action must be 'approve' or 'reject'")]
        public string Action { get; set; } = string.Empty;

        public string? Reason { get; set; } // Required for reject action
    }
}