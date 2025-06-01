// Models/DTOs/ViewTracking/ViewTrackingCreateDto.cs
using KDomBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.ViewTracking
{
    public class ViewTrackingCreateDto
    {
        [Required]
        public ContentType ContentType { get; set; }

        [Required]
        public string ContentId { get; set; } = string.Empty;

        public int? ViewerId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}