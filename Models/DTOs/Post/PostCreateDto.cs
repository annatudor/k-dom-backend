namespace KDomBackend.Models.DTOs.Post
{
    public class PostCreateDto
    {
        public string ContentHtml { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

}
