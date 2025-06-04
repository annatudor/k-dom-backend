namespace KDomBackend.Models.DTOs.Collaboration
{
    public class BulkActionResultDto
    {
        public string RequestId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}