namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomEditReadDto
    {
        public string Id { get; set; } = string.Empty;
        public string EditNote { get; set; } = string.Empty;
        public bool IsMinor { get; set; }
        public DateTime EditedAt { get; set; }
    }
}
