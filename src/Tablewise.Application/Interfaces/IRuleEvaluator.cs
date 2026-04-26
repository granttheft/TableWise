namespace Tablewise.Application.Interfaces;

/// <summary>
/// Kural motoru interface (Faz 3'te implemente edilecek).
/// Rezervasyon isteklerini venue'nin kurallarına göre değerlendirir.
/// </summary>
public interface IRuleEvaluator
{
    /// <summary>
    /// Rezervasyon isteğini değerlendirir.
    /// </summary>
    /// <param name="context">Değerlendirme context'i</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Değerlendirme sonucu</returns>
    Task<RuleEvaluationResult> EvaluateAsync(
        RuleEvaluationContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Kural değerlendirme context'i.
/// </summary>
public sealed record RuleEvaluationContext
{
    /// <summary>
    /// Mekan ID.
    /// </summary>
    public Guid VenueId { get; init; }

    /// <summary>
    /// Müşteri ID (kayıtlı ise).
    /// </summary>
    public Guid? CustomerId { get; init; }

    /// <summary>
    /// Müşteri email.
    /// </summary>
    public string? CustomerEmail { get; init; }

    /// <summary>
    /// Müşteri telefon.
    /// </summary>
    public string? CustomerPhone { get; init; }

    /// <summary>
    /// Müşteri tier (Regular, Gold, VIP, Blacklisted).
    /// </summary>
    public string? CustomerTier { get; init; }

    /// <summary>
    /// Rezervasyon tarihi/saati (UTC).
    /// </summary>
    public DateTime ReservedFor { get; init; }

    /// <summary>
    /// Kişi sayısı.
    /// </summary>
    public int PartySize { get; init; }

    /// <summary>
    /// Hedef masa ID (opsiyonel).
    /// </summary>
    public Guid? TableId { get; init; }

    /// <summary>
    /// Hedef masa birleşimi ID (opsiyonel).
    /// </summary>
    public Guid? TableCombinationId { get; init; }

    /// <summary>
    /// Custom field yanıtları.
    /// </summary>
    public Dictionary<string, string>? CustomFieldAnswers { get; init; }

    /// <summary>
    /// Rezervasyon kaynağı.
    /// </summary>
    public string Source { get; init; } = "BookingUI";
}

/// <summary>
/// Kural değerlendirme sonucu.
/// </summary>
public sealed record RuleEvaluationResult
{
    /// <summary>
    /// İstek kabul edildi mi?
    /// </summary>
    public bool IsAllowed { get; init; } = true;

    /// <summary>
    /// Engelleme nedeni (IsAllowed=false ise).
    /// </summary>
    public string? BlockReason { get; init; }

    /// <summary>
    /// Uygulanacak indirim yüzdesi.
    /// </summary>
    public decimal? DiscountPercent { get; init; }

    /// <summary>
    /// Kapora gerekli mi?
    /// </summary>
    public bool RequiresDeposit { get; init; }

    /// <summary>
    /// Kapora tutarı (kapora gerekli ise).
    /// </summary>
    public decimal? DepositAmount { get; init; }

    /// <summary>
    /// Tetiklenen kuralların snapshot'ı (JSON olarak serialize edilecek).
    /// </summary>
    public IReadOnlyList<AppliedRuleSnapshot> AppliedRules { get; init; } = [];

    /// <summary>
    /// Uyarı mesajları (UI'da gösterilecek).
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];

    /// <summary>
    /// Varsayılan izin ver sonucu (kural motoru henüz implemente değil).
    /// </summary>
    public static RuleEvaluationResult Allow()
        => new()
        {
            IsAllowed = true,
            AppliedRules = []
        };

    /// <summary>
    /// Engelle sonucu.
    /// </summary>
    public static RuleEvaluationResult Block(string reason)
        => new()
        {
            IsAllowed = false,
            BlockReason = reason
        };
}

/// <summary>
/// Uygulanan kural snapshot'ı.
/// </summary>
public sealed record AppliedRuleSnapshot
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
    /// Uygulanan aksiyon tipi.
    /// </summary>
    public string ActionType { get; init; } = string.Empty;

    /// <summary>
    /// Aksiyon parametreleri.
    /// </summary>
    public Dictionary<string, object>? ActionParams { get; init; }
}
