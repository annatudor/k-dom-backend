using KDomBackend.Models.DTOs.Common;
using KDomBackend.Models.DTOs.Post;

namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomDiscussionReadDto
    {
        public KDomBasicInfoDto KDom { get; set; } = new();
        public PagedResult<PostReadDto> Posts { get; set; } = new();
        public KDomDiscussionStatsDto Stats { get; set; } = new();
    }
}
