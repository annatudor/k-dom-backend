using System.ComponentModel.DataAnnotations;
using KDomBackend.Enums;

public class KDomUpdateMetadataDto
{
    [Required]
    public string KDomId { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;
    public string? ParentId { get; set; }

    public string Description { get; set; } = string.Empty;
    public Hub Hub { get; set; }
    public Language Language { get; set; } 
    public bool IsForKids { get; set; }
    public KDomTheme Theme { get; set; } = KDomTheme.Light;
    public DateTime UpdatedAt { get; set; }
}
