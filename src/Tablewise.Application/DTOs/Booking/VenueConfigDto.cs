namespace Tablewise.Application.DTOs.Booking;

/// <summary>
/// Booking UI için mekan yapılandırma DTO.
/// Public endpoint'ten döner, hassas bilgi içermez.
/// </summary>
public sealed record VenueConfigDto
{
    /// <summary>
    /// Mekan ID.
    /// </summary>
    public Guid VenueId { get; init; }

    /// <summary>
    /// Mekan adı.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Tenant slug (URL'de kullanılır).
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Mekan açıklaması.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Logo URL.
    /// </summary>
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Mekan adresi.
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Mekan telefonu.
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Slot süresi (dakika).
    /// </summary>
    public int SlotDurationMinutes { get; init; }

    /// <summary>
    /// Kapora modülü aktif mi?
    /// </summary>
    public bool DepositEnabled { get; init; }

    /// <summary>
    /// Kapora tutarı.
    /// </summary>
    public decimal? DepositAmount { get; init; }

    /// <summary>
    /// Kapora kişi başı mı?
    /// </summary>
    public bool DepositPerPerson { get; init; }

    /// <summary>
    /// Çalışma saatleri.
    /// </summary>
    public Dictionary<string, WorkingHoursPeriod>? WorkingHours { get; init; }

    /// <summary>
    /// Custom field'lar (rezervasyon formundaki ek alanlar).
    /// </summary>
    public IReadOnlyList<VenueCustomFieldDto> CustomFields { get; init; } = [];

    /// <summary>
    /// Minimum kişi sayısı.
    /// </summary>
    public int MinPartySize { get; init; } = 1;

    /// <summary>
    /// Maksimum kişi sayısı.
    /// </summary>
    public int MaxPartySize { get; init; } = 20;

    /// <summary>
    /// Minimum kaç gün öncesinden rezervasyon yapılabilir.
    /// </summary>
    public int MinAdvanceBookingDays { get; init; } = 0;

    /// <summary>
    /// Maksimum kaç gün öncesinden rezervasyon yapılabilir.
    /// </summary>
    public int MaxAdvanceBookingDays { get; init; } = 30;
}

/// <summary>
/// Çalışma saatleri periyodu.
/// </summary>
public sealed record WorkingHoursPeriod
{
    /// <summary>
    /// Açılış saati (HH:mm formatında).
    /// </summary>
    public string Open { get; init; } = string.Empty;

    /// <summary>
    /// Kapanış saati (HH:mm formatında).
    /// </summary>
    public string Close { get; init; } = string.Empty;

    /// <summary>
    /// Gün kapalı mı?
    /// </summary>
    public bool IsClosed { get; init; }
}

/// <summary>
/// Booking UI için custom field DTO.
/// </summary>
public sealed record VenueCustomFieldDto
{
    /// <summary>
    /// Field ID.
    /// </summary>
    public Guid FieldId { get; init; }

    /// <summary>
    /// Field adı.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Field tipi (Text, Number, Select, Checkbox).
    /// </summary>
    public string FieldType { get; init; } = string.Empty;

    /// <summary>
    /// Zorunlu mu?
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Seçenekler (Select tipi için).
    /// </summary>
    public IReadOnlyList<string>? Options { get; init; }

    /// <summary>
    /// Placeholder text.
    /// </summary>
    public string? Placeholder { get; init; }
}
