using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomEditDto
    {
        [Required]
        public string KDomSlug { get; set; } = string.Empty;

        public string ContentHtml { get; set; } = string.Empty;

        public string? EditNote { get; set; }
        public bool IsMinor { get; set; } = false;
        public bool IsAutoSave { get; set; } = true;

    }

}
