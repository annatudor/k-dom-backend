namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public string Description { get; set; } = string.Empty;
        public string Hub { get; set; } = string.Empty;
        public bool IsForKids { get; set; } = false;
        public string Theme { get; set; } = "light";
        public string ContentHtml { get; set; } = string.Empty;
    }

}
