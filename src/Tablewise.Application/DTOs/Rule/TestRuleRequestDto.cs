namespace Tablewise.Application.DTOs.Rule;

/// <summary>
/// Kural test istek DTO.
/// Kural testleri için simüle edilmiş context bilgileri içerir.
/// </summary>
public sealed record TestRuleRequestDto
{
    /// <summary>
    /// Kişi sayısı.
    /// </summary>
    public int PartySize { get; init; }

    /// <summary>
    /// Rezervasyon kaç gün öncesinden yapılıyor.
    /// </summary>
    public int DaysInAdvance { get; init; }

    /// <summary>
    /// Müşteri tier seviyesi (opsiyonel).
    /// Geçerli değerler: "Regular", "Gold", "VIP", "Blacklisted"
    /// </summary>
    public string? CustomerTier { get; init; }

    /// <summary>
    /// Müşteri toplam ziyaret sayısı (opsiyonel).
    /// </summary>
    public int? CustomerTotalVisits { get; init; }

    /// <summary>
    /// Rezervasyon günü (opsiyonel).
    /// Geçerli değerler: "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
    /// </summary>
    public string? DayOfWeek { get; init; }

    /// <summary>
    /// Rezervasyon saati (0-23).
    /// </summary>
    public int Hour { get; init; }

    /// <summary>
    /// Mekan doluluk oranı (0.0-1.0).
    /// </summary>
    public double VenueOccupancy { get; init; }

    /// <summary>
    /// Masa kapasitesi (opsiyonel).
    /// </summary>
    public int? TableCapacity { get; init; }

    /// <summary>
    /// Masa lokasyonu (opsiyonel).
    /// </summary>
    public string? TableLocation { get; init; }

    /// <summary>
    /// Grup kompozisyonu (opsiyonel).
    /// Geçerli değerler: "Mixed", "AllMale", "AllFemale", "Family"
    /// </summary>
    public string? GroupComposition { get; init; }

    /// <summary>
    /// Gruptaki erkek sayısı (opsiyonel).
    /// </summary>
    public int? MaleCount { get; init; }

    /// <summary>
    /// Gruptaki kadın sayısı (opsiyonel).
    /// </summary>
    public int? FemaleCount { get; init; }
}
