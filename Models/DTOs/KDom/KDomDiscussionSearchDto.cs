using KDomBackend.Models.DTOs.Common;

namespace KDomBackend.Models.DTOs.KDom
{
    /// <summary>
    /// DTO pentru căutarea în discussion-ul unui K-Dom
    /// </summary>
    public class KDomDiscussionSearchDto : PagedFilterDto
    {
        /// <summary>
        /// Text de căutat în conținutul postărilor
        /// </summary>
        public string? ContentQuery { get; set; }

        /// <summary>
        /// Username de căutat
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Sortare (newest, oldest, most-liked)
        /// </summary>
        public string SortBy { get; set; } = "newest";

        /// <summary>
        /// Filtrare doar postările cu like-uri (opțional)
        /// </summary>
        public bool? OnlyLiked { get; set; }

        /// <summary>
        /// Postări din ultima perioadă (în zile)
        /// </summary>
        public int? LastDays { get; set; }
    }
}