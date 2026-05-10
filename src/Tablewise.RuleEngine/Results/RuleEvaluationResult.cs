using System.Text.Json;
using Tablewise.Domain.Entities;

namespace Tablewise.RuleEngine.Results;

/// <summary>
/// Kural motoru pipeline'ının toplam sonucu.
/// Tüm kuralların değerlendirilmesi sonrasında oluşturulur.
/// Application.RuleEvaluationResult ile karıştırılmamalı.
/// </summary>
public sealed class PipelineResult
{
    /// <summary>
    /// Rezervasyon engellendi mi? (Block aksiyonu varsa true).
    /// </summary>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// Engelleme mesajı (IsBlocked=true ise).
    /// </summary>
    public string? BlockReason { get; set; }

    /// <summary>
    /// Uygulanan tüm kural sonuçları.
    /// </summary>
    public List<RuleOutcome> Outcomes { get; set; } = [];

    /// <summary>
    /// Tercih edilen pozisyon/lokasyon (kural önerisi).
    /// </summary>
    public string? PreferredPosition { get; set; }

    /// <summary>
    /// Toplam indirim yüzdesi (birden fazla Discount kuralı varsa toplanır).
    /// </summary>
    public decimal TotalDiscountPercent { get; set; }

    /// <summary>
    /// Kapora gerekli mi?
    /// </summary>
    public bool RequiresDeposit { get; set; }

    /// <summary>
    /// Kapora tutarı (RequiresDeposit=true ise).
    /// </summary>
    public decimal? DepositAmount { get; set; }

    /// <summary>
    /// Önerilen alternatif masalar.
    /// </summary>
    public List<Table> SuggestedAlternatives { get; set; } = [];

    /// <summary>
    /// Uyarı mesajları (kullanıcıya gösterilir, engellemez).
    /// </summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// Bilgi mesajları (debug/logging için).
    /// </summary>
    public List<string> Infos { get; set; } = [];

    /// <summary>
    /// Uygulanan kuralların JSON snapshot'ı.
    /// Reservation.AppliedRulesSnapshot alanına yazılır.
    /// </summary>
    public string AppliedRulesSnapshotJson => JsonSerializer.Serialize(
        Outcomes.Select(o => new
        {
            o.RuleId,
            o.RuleName,
            ActionType = o.ActionType.ToString(),
            o.Message,
            o.Payload,
            o.EvaluatedAt
        }),
        new JsonSerializerOptions { WriteIndented = false });

    /// <summary>
    /// Sonucu başarılı (izin verildi) olarak oluşturur.
    /// </summary>
    public static PipelineResult Allow() => new()
    {
        IsBlocked = false
    };

    /// <summary>
    /// Sonucu engellenmiş olarak oluşturur.
    /// </summary>
    /// <param name="reason">Engelleme nedeni</param>
    public static PipelineResult Block(string reason) => new()
    {
        IsBlocked = true,
        BlockReason = reason
    };

    /// <summary>
    /// Sonuca outcome ekler.
    /// </summary>
    public void AddOutcome(RuleOutcome outcome)
    {
        Outcomes.Add(outcome);
    }

    /// <summary>
    /// Sonuca uyarı ekler.
    /// </summary>
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    /// <summary>
    /// Sonuca bilgi ekler.
    /// </summary>
    public void AddInfo(string info)
    {
        Infos.Add(info);
    }
}
