using System.ComponentModel.DataAnnotations;

public class KDomUpdateMetadataDto
{
    [Required]
    public string KDomId { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string Hub { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public bool IsForKids { get; set; }
    public string Theme { get; set; } = "light";
    public DateTime UpdatedAt { get; set; }
}
