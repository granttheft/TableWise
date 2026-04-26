using System.ComponentModel.DataAnnotations;

namespace Tablewise.Application.DTOs.Booking;

/// <summary>
/// Müsaitlik sorgusu istek DTO.
/// </summary>
public sealed record AvailabilityRequestDto
{
    /// <summary>
    /// Tarih (YYYY-MM-DD formatında).
    /// </summary>
    [Required(ErrorMessage = "Tarih zorunludur.")]
    public DateTime Date { get; init; }

    /// <summary>
    /// Kişi sayısı.
    /// </summary>
    [Required(ErrorMessage = "Kişi sayısı zorunludur.")]
    [Range(1, 50, ErrorMessage = "Kişi sayısı 1-50 arasında olmalıdır.")]
    public int PartySize { get; init; }

    /// <summary>
    /// Belirli bir masa için kontrol (opsiyonel).
    /// </summary>
    public Guid? TableId { get; init; }
}

/// <summary>
/// Müsaitlik sorgusu response DTO.
/// </summary>
public sealed record AvailabilityResponseDto
{
    /// <summary>
    /// Mekan ID.
    /// </summary>
    public Guid VenueId { get; init; }

    /// <summary>
    /// Sorgulanan tarih.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Sorgulanan kişi sayısı.
    /// </summary>
    public int PartySize { get; init; }

    /// <summary>
    /// Müsait slotlar.
    /// </summary>
    public IReadOnlyList<SlotDto> Slots { get; init; } = [];

    /// <summary>
    /// Mekan o gün kapalı mı?
    /// </summary>
    public bool IsVenueClosed { get; init; }

    /// <summary>
    /// Kapalılık nedeni (kapalı ise).
    /// </summary>
    public string? ClosureReason { get; init; }
}

/// <summary>
/// Slot DTO.
/// </summary>
public sealed record SlotDto
{
    /// <summary>
    /// Slot başlangıç zamanı (ISO 8601).
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// Slot bitiş zamanı (ISO 8601).
    /// </summary>
    public DateTime EndTime { get; init; }

    /// <summary>
    /// Slot saat string'i (HH:mm formatında, yerel saat).
    /// </summary>
    public string TimeLabel { get; init; } = string.Empty;

    /// <summary>
    /// Müsait masa sayısı.
    /// </summary>
    public int AvailableTableCount { get; init; }

    /// <summary>
    /// Müsait masalar.
    /// </summary>
    public IReadOnlyList<TableOptionDto>? AvailableTables { get; init; }

    /// <summary>
    /// Müsait masa birleşimleri.
    /// </summary>
    public IReadOnlyList<TableCombinationOptionDto>? AvailableCombinations { get; init; }
}

/// <summary>
/// Masa seçenek DTO.
/// </summary>
public sealed record TableOptionDto
{
    /// <summary>
    /// Masa ID.
    /// </summary>
    public Guid TableId { get; init; }

    /// <summary>
    /// Masa adı.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Kapasite.
    /// </summary>
    public int Capacity { get; init; }

    /// <summary>
    /// Lokasyon.
    /// </summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>
    /// Açıklama.
    /// </summary>
    public string? Description { get; init; }
}

/// <summary>
/// Masa birleşimi seçenek DTO.
/// </summary>
public sealed record TableCombinationOptionDto
{
    /// <summary>
    /// Birleşim ID.
    /// </summary>
    public Guid CombinationId { get; init; }

    /// <summary>
    /// Birleşim adı.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Toplam kapasite.
    /// </summary>
    public int CombinedCapacity { get; init; }
}
