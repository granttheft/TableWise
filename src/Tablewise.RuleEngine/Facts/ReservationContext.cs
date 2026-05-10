using Tablewise.Domain.Entities;

namespace Tablewise.RuleEngine.Facts;

/// <summary>
/// Kural motoruna girecek tüm bilgiyi içeren context.
/// Rezervasyon oluşturma sırasında kural değerlendirmesi için gerekli tüm veriler burada bulunur.
/// </summary>
public sealed class ReservationContext
{
    #region Core Entities (Readonly snapshots)

    /// <summary>
    /// Tenant bilgisi (snapshot, readonly).
    /// </summary>
    public required Tenant Tenant { get; init; }

    /// <summary>
    /// Mekan bilgisi.
    /// </summary>
    public required Venue Venue { get; init; }

    /// <summary>
    /// Rezervasyon taslağı (henüz kaydedilmedi).
    /// </summary>
    public required Reservation Reservation { get; init; }

    /// <summary>
    /// Atanmış masa (nullable - henüz atanmamışsa).
    /// </summary>
    public Table? Table { get; init; }

    /// <summary>
    /// Masa birleşimi (nullable).
    /// </summary>
    public TableCombination? TableCombination { get; init; }

    /// <summary>
    /// Müşteri bilgisi (nullable - misafir kayıtsız rezervasyon yapabilir).
    /// </summary>
    public Customer? Customer { get; init; }

    #endregion

    #region Computed Metrics

    /// <summary>
    /// Bu slot için doluluk oranı (0.0 - 1.0).
    /// </summary>
    public double CurrentOccupancyRate { get; init; }

    /// <summary>
    /// Rezervasyon kaç gün öncesinden yapılıyor.
    /// </summary>
    public int DaysInAdvance { get; init; }

    /// <summary>
    /// Değerlendirme zamanı (UTC).
    /// </summary>
    public DateTime EvaluatedAt { get; init; } = DateTime.UtcNow;

    #endregion

    #region Grup Kompozisyonu (Tümü opsiyonel)

    /// <summary>
    /// Gruptaki erkek sayısı (opsiyonel - müşteri doldurmadıysa null).
    /// </summary>
    public int? MaleCount { get; init; }

    /// <summary>
    /// Gruptaki kadın sayısı (opsiyonel - müşteri doldurmadıysa null).
    /// </summary>
    public int? FemaleCount { get; init; }

    /// <summary>
    /// Grup kompozisyonu.
    /// Geçerli değerler: "Mixed", "AllMale", "AllFemale", "Family"
    /// </summary>
    public string? GroupComposition { get; init; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Erken rezervasyon mu? (7+ gün öncesinden).
    /// </summary>
    public bool IsEarlyBooking => DaysInAdvance >= 7;

    /// <summary>
    /// Yoğun saat mi? (18:00-22:00 arası).
    /// </summary>
    public bool IsPeakHour
    {
        get
        {
            var hour = Reservation.ReservedFor.Hour;
            return hour >= 18 && hour < 22;
        }
    }

    /// <summary>
    /// Kadın oranı (0.0 - 1.0, null ise hesaplanamadı).
    /// </summary>
    public double? FemaleRatio
    {
        get
        {
            if (!FemaleCount.HasValue || Reservation.PartySize <= 0)
                return null;
            return (double)FemaleCount.Value / Reservation.PartySize;
        }
    }

    /// <summary>
    /// Erkek oranı (0.0 - 1.0, null ise hesaplanamadı).
    /// </summary>
    public double? MaleRatio
    {
        get
        {
            if (!MaleCount.HasValue || Reservation.PartySize <= 0)
                return null;
            return (double)MaleCount.Value / Reservation.PartySize;
        }
    }

    /// <summary>
    /// Hafta sonu mu? (Cumartesi veya Pazar).
    /// </summary>
    public bool IsWeekend
    {
        get
        {
            var dayOfWeek = Reservation.ReservedFor.DayOfWeek;
            return dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;
        }
    }

    /// <summary>
    /// Büyük grup mu? (8+ kişi).
    /// </summary>
    public bool IsLargeGroup => Reservation.PartySize >= 8;

    #endregion
}
