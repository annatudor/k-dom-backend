using KDomBackend.Enums;

namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomMetadataEditReadDto
    {
        public string Id { get; set; } = string.Empty;
        public string? PreviousParentId { get; set; }
        public string PreviousTitle { get; set; } = string.Empty;
        public string PreviousDescription { get; set; } = string.Empty;
        public Language PreviousLanguage { get; set; }
        public Hub PreviousHub { get; set; }
        public bool PreviousIsForKids { get; set; }
        public KDomTheme PreviousTheme { get; set; } = KDomTheme.Light;
        public DateTime EditedAt { get; set; }
    }
}
