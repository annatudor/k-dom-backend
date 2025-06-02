namespace KDomBackend.Models.DTOs.ViewTracking
{
    public class BatchTrackingResultDto
    {
        public string Message { get; set; } = string.Empty;
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<TrackingResultDto> Results { get; set; } = new();
    }

    public class TrackingResultDto
    {
        public string ContentId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
}