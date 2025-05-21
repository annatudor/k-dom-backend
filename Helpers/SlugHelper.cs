using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public static class SlugHelper
{
    public static string GenerateSlug(string phrase)
    {
        string normalized = phrase.ToLowerInvariant();

        
        normalized = RemoveDiacritics(normalized);

        
        normalized = Regex.Replace(normalized, @"[^a-z0-9\s-]", ""); 
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();  
        normalized = Regex.Replace(normalized, @"\s", "-");          

        return normalized;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
