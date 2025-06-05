using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.Moderation
{
    public class BulkModerationDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one K-Dom ID is required.")]
        [MaxLength(50, ErrorMessage = "Maximum 50 K-Doms can be processed at once.")]
        public List<string> KDomIds { get; set; } = new();

        [Required]
        [RegularExpression("^(approve|reject)$", ErrorMessage = "Action must be 'approve' or 'reject'")]
        public string Action { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string? Reason { get; set; } // Required pentru reject

        public bool DeleteRejected { get; set; } = true; // Pentru reject
    }

    public class BulkModerationResultDto
    {
        public string Message { get; set; } = string.Empty;
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int TotalProcessed { get; set; }
        public List<ModerationResultItemDto> Results { get; set; } = new();
    }

    public class ModerationResultItemDto
    {
        public string KDomId { get; set; } = string.Empty;
        public string KDomTitle { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Error { get; set; }
    }
}