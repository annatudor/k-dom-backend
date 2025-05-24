using KDomBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomSubCreateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string ContentHtml { get; set; } = string.Empty;

        public KDomTheme Theme { get; set; } = KDomTheme.Light;
    }
}
