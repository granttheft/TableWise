using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Tablewise.Infrastructure.Auth;

/// <summary>
/// URL-friendly slug üreten yardımcı sınıf.
/// Türkçe karakter desteği ile.
/// </summary>
public static partial class SlugGenerator
{
    /// <summary>
    /// Türkçe → ASCII karakter haritası.
    /// </summary>
    private static readonly Dictionary<char, string> TurkishCharMap = new()
    {
        { 'ı', "i" }, { 'İ', "i" },
        { 'ğ', "g" }, { 'Ğ', "g" },
        { 'ü', "u" }, { 'Ü', "u" },
        { 'ş', "s" }, { 'Ş', "s" },
        { 'ö', "o" }, { 'Ö', "o" },
        { 'ç', "c" }, { 'Ç', "c" }
    };

    /// <summary>
    /// Text'ten URL-friendly slug üretir.
    /// </summary>
    /// <param name="text">Orijinal text</param>
    /// <returns>Slug</returns>
    public static string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Türkçe karakterleri dönüştür
        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (TurkishCharMap.TryGetValue(c, out var replacement))
            {
                sb.Append(replacement);
            }
            else
            {
                sb.Append(c);
            }
        }

        var result = sb.ToString();

        // Lowercase
        result = result.ToLowerInvariant();

        // Diacritics kaldır (Türkçe dışındaki karakterler için)
        result = RemoveDiacritics(result);

        // Alfanumerik olmayan karakterleri tire ile değiştir
        result = NonAlphanumericRegex().Replace(result, "-");

        // Birden fazla tireyi teke indir
        result = MultipleDashRegex().Replace(result, "-");

        // Baş ve sondaki tireleri kaldır
        result = result.Trim('-');

        return result;
    }

    /// <summary>
    /// Slug'ı benzersiz yapar (suffix ekleyerek).
    /// </summary>
    /// <param name="baseSlug">Temel slug</param>
    /// <param name="suffix">Eklenecek numara</param>
    /// <returns>Benzersiz slug</returns>
    public static string MakeUnique(string baseSlug, int suffix)
    {
        return $"{baseSlug}-{suffix}";
    }

    /// <summary>
    /// Diacritics (aksanlı karakterler) kaldırır.
    /// </summary>
    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleDashRegex();
}
