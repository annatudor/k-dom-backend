using KDomBackend.Models.DTOs.Common;
using KDomBackend.Models.DTOs.Post;

namespace KDomBackend.Models.DTOs.KDom
{
    /// <summary>
    /// DTO pentru parametrii de paginare discussion
    /// </summary>
    public class KDomDiscussionFilterDto : PagedFilterDto
    {
        // Moștenește Page și PageSize din PagedFilterDto
        // Poate fi extins cu alte filtre în viitor (ex: sortBy, dateFrom, etc.)
    }
}