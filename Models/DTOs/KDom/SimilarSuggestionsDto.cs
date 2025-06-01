namespace KDomBackend.Models.DTOs.KDom
{
    public class SimilarSuggestionsDto
    {
        public List<string> SimilarTitles { get; set; } = new();
        public List<KDomTagSearchResultDto> RelatedKDoms { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public bool HasSuggestions => SimilarTitles.Any() || RelatedKDoms.Any();
    }
}