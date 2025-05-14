namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomMetadataEditReadDto
    {
        public string Id { get; set; } = string.Empty;
        public string PreviousTitle { get; set; } = string.Empty;
        public string PreviousDescription { get; set; } = string.Empty;
        public string PreviousLanguage { get; set; } = string.Empty;
        public string PreviousHub { get; set; } = string.Empty;
        public bool PreviousIsForKids { get; set; }
        public string PreviousTheme { get; set; } = "light";
        public DateTime EditedAt { get; set; }
    }
}
