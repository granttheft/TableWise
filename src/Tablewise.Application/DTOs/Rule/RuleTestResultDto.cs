namespace Tablewise.Application.DTOs.Rule;

/// <summary>
/// Kural test sonucu DTO.
/// </summary>
public sealed record RuleTestResultDto
{
    /// <summary>
    /// Kural tetiklendi mi?
    /// </summary>
    public bool Triggered { get; init; }

    /// <summary>
    /// Kural outcome'u (tetiklendiyse).
    /// </summary>
    public RuleOutcomeDto? Outcome { get; init; }

    /// <summary>
    /// Çalışma süresi (milisaniye).
    /// </summary>
    public int ExecutionMs { get; init; }

    /// <summary>
    /// Koşul değerlendirme detayları (debug için).
    /// Sadece custom_condition kuralları için dolu.
    /// </summary>
    public List<ConditionEvaluationDto>? ConditionEvaluations { get; init; }
}

/// <summary>
/// Kural outcome DTO.
/// </summary>
public sealed record RuleOutcomeDto
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
    /// Aksiyon tipi.
    /// </summary>
    public string ActionType { get; init; } = string.Empty;

    /// <summary>
    /// Mesaj (kullanıcıya gösterilecek).
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Payload (JSON, aksiyon detayları).
    /// </summary>
    public string? Payload { get; init; }
}

/// <summary>
/// Koşul değerlendirme detayı DTO.
/// </summary>
public sealed record ConditionEvaluationDto
{
    /// <summary>
    /// Alan adı.
    /// </summary>
    public string Field { get; init; } = string.Empty;

    /// <summary>
    /// Operatör.
    /// </summary>
    public string Op { get; init; } = string.Empty;

    /// <summary>
    /// Beklenen değer.
    /// </summary>
    public object? ExpectedValue { get; init; }

    /// <summary>
    /// Gerçek değer.
    /// </summary>
    public object? ActualValue { get; init; }

    /// <summary>
    /// Koşul sonucu.
    /// </summary>
    public bool Result { get; init; }
}
