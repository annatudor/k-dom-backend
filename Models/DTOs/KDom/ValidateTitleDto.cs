using System.ComponentModel.DataAnnotations;

namespace KDomBackend.Models.DTOs.KDom
{
    public class ValidateTitleDto
    {
        [Required]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters long.")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;
    }
}
