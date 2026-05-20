namespace Tablewise.Application.Common;

/// <summary>
/// API'den gelen DateTime değerlerini PostgreSQL timestamptz ile uyumlu UTC'ye çevirir.
/// </summary>
public static class DateTimeNormalization
{
    private static readonly TimeZoneInfo IstanbulTimeZone = ResolveIstanbulTimeZone();

    /// <summary>
    /// Rezervasyon tarih/saatini UTC'ye normalize eder.
    /// Unspecified değerler Europe/Istanbul yerel saat kabul edilir.
    /// </summary>
    public static DateTime ToUtcReservedFor(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(value, DateTimeKind.Unspecified),
                IstanbulTimeZone),
        };
    }

    private static TimeZoneInfo ResolveIstanbulTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
    }
}
