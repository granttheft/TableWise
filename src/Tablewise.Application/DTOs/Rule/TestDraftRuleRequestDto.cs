namespace Tablewise.Application.DTOs.Rule;

/// <summary>
/// Kaydedilmemiş (taslak) kuralı test etmek için istek DTO.
/// </summary>
public sealed record TestDraftRuleRequestDto
{
    /// <summary>
    /// Kural tipi (örn. custom_condition).
    /// </summary>
    public string RuleType { get; init; } = "custom_condition";

    /// <summary>
    /// Koşullar JSON (version alanı zorunlu).
    /// </summary>
    public string ConditionsJson { get; init; } = string.Empty;

    /// <summary>
    /// Aksiyonlar JSON (version alanı zorunlu).
    /// </summary>
    public string ActionsJson { get; init; } = string.Empty;

    /// <summary>
    /// Test sonucu gösteriminde kullanılacak kural adı (opsiyonel).
    /// </summary>
    public string? RuleName { get; init; }

    /// <summary>
    /// Simüle edilmiş rezervasyon bağlamı.
    /// </summary>
    public required TestRuleRequestDto Context { get; init; }
}
