namespace Tablewise.Application.DTOs.Rule;

/// <summary>
/// Kural şablonu DTO.
/// </summary>
public sealed record RuleTemplateDto
{
    /// <summary>
    /// Şablon ID.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Şablon adı.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Şablon açıklaması.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Şablon ikonu.
    /// </summary>
    public string Icon { get; init; } = string.Empty;

    /// <summary>
    /// Kategori.
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Varsayılan koşullar JSON.
    /// </summary>
    public string DefaultConditionsJson { get; init; } = string.Empty;

    /// <summary>
    /// Varsayılan aksiyonlar JSON.
    /// </summary>
    public string DefaultActionsJson { get; init; } = string.Empty;

    /// <summary>
    /// Parametreler şeması (UI form oluşturmak için).
    /// </summary>
    public Dictionary<string, object> ParamsSchema { get; init; } = new();
}

/// <summary>
/// Kural istatistik DTO.
/// </summary>
public sealed record RuleStatDto
{
    /// <summary>
    /// Kural ID.
    /// </summary>
    public Guid RuleId { get; init; }

    /// <summary>
    /// Kural adı.
    /// </summary>
    public string RuleName { get; init; } = string.Empty;

    /// <summary>
    /// Kaç kez tetiklendiği.
    /// </summary>
    public int TimesTriggered { get; init; }

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; init; }
}
