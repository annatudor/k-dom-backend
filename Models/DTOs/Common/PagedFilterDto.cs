namespace KDomBackend.Models.DTOs.Common
{
    public class PagedFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
