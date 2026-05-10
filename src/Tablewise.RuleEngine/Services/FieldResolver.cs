using Microsoft.Extensions.Logging;
using Tablewise.RuleEngine.Facts;

namespace Tablewise.RuleEngine.Services;

/// <summary>
/// Güvenli field resolver.
/// ReservationContext'ten alan değerlerini çözer.
/// Switch/dictionary bazlı - reflection veya eval KULLANMAZ.
/// </summary>
public static class FieldResolver
{
    /// <summary>
    /// İzin verilen alan adları (whitelist).
    /// Güvenlik için sadece bu alanlar çözülebilir.
    /// </summary>
    public static readonly HashSet<string> AllowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        // Temel alanlar
        "partySize",
        "daysInAdvance",
        
        // Müşteri alanları
        "customer.tier",
        "customer.totalVisits",
        
        // Rezervasyon alanları
        "reservation.reservedFor.hour",
        "reservation.reservedFor.dayOfWeek",
        
        // Mekan alanları
        "venue.currentOccupancy",
        
        // Masa alanları
        "table.capacity",
        "table.location",
        
        // Grup kompozisyonu alanları
        "groupComposition",
        "femaleRatio",
        "maleRatio",
        "maleCount",
        "femaleCount"
    };

    /// <summary>
    /// Belirtilen alanın değerini ReservationContext'ten çözer.
    /// </summary>
    /// <param name="context">Rezervasyon context'i</param>
    /// <param name="field">Alan adı</param>
    /// <param name="logger">Logger (opsiyonel, bilinmeyen field için)</param>
    /// <returns>Alan değeri veya null (bilinmeyen/null ise)</returns>
    public static object? GetFieldValue(ReservationContext context, string field, ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(field))
            return null;

        var normalizedField = field.Trim().ToLowerInvariant();

        return normalizedField switch
        {
            // Temel alanlar
            "partysize" => context.Reservation.PartySize,
            "daysinadvance" => context.DaysInAdvance,

            // Müşteri alanları
            "customer.tier" => context.Customer?.Tier.ToString(),
            "customer.totalvisits" => context.Customer?.TotalVisits,

            // Rezervasyon alanları
            "reservation.reservedfor.hour" => context.Reservation.ReservedFor.Hour,
            "reservation.reservedfor.dayofweek" => context.Reservation.ReservedFor.DayOfWeek.ToString(),

            // Mekan alanları
            "venue.currentoccupancy" => context.CurrentOccupancyRate,

            // Masa alanları
            "table.capacity" => context.Table?.Capacity,
            "table.location" => context.Table?.Location,

            // Grup kompozisyonu alanları
            "groupcomposition" => context.GroupComposition,
            "femaleratio" => context.FemaleRatio,
            "maleratio" => context.MaleRatio,
            "malecount" => context.MaleCount,
            "femalecount" => context.FemaleCount,

            // Bilinmeyen alan
            _ => LogUnknownFieldAndReturnNull(field, logger)
        };
    }

    /// <summary>
    /// Alan adının whitelist'te olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="field">Alan adı</param>
    /// <returns>Whitelist'te ise true</returns>
    public static bool IsFieldAllowed(string field)
    {
        if (string.IsNullOrWhiteSpace(field))
            return false;

        return AllowedFields.Contains(field.Trim());
    }

    /// <summary>
    /// Bilinmeyen field için null döner ve loglar.
    /// </summary>
    private static object? LogUnknownFieldAndReturnNull(string field, ILogger? logger)
    {
        logger?.LogWarning(
            "Bilinmeyen field talep edildi: {Field}. Güvenlik için null dönülüyor.",
            field);
        return null;
    }
}
