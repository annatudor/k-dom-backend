using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.Auth
{
    public class GoogleAuthCodeDto
    {
        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
