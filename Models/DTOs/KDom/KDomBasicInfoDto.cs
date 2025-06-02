namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomBasicInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AuthorUsername { get; set; } = string.Empty;
        public int FollowersCount { get; set; }
    }
}
