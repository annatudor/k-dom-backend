using Microsoft.AspNetCore.Routing.Constraints;
using KDomBackend.Enums;

namespace KDomBackend.Models.Entities
{
    public class Flag
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ContentType ContentType { get; set; } 
        public string ContentId { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsResolved { get; set; } = false;

    }
}
