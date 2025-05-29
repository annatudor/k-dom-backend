using Microsoft.AspNetCore.Routing.Constraints;
using KDomBackend.Enums;
using Spryer;

namespace KDomBackend.Models.Entities
{
    public class Flag
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DbEnum<ContentType> ContentType { get; set; } 
        public string ContentId { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsResolved { get; set; } = false;

    }
}
