using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.KDom
{
    public class KDomRejectDto
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}
