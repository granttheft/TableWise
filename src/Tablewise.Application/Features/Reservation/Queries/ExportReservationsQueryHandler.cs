using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Reservation.Queries;

/// <summary>
/// ExportReservationsQuery handler.
/// </summary>
public sealed class ExportReservationsQueryHandler : IRequestHandler<ExportReservationsQuery, ExportReservationsResult>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public ExportReservationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<ExportReservationsResult> Handle(ExportReservationsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Reservations
            .Query()
            .Include(r => r.Venue)
            .Include(r => r.Table)
            .Include(r => r.Customer)
            .AsQueryable();

        // Filters
        if (request.VenueId.HasValue)
        {
            query = query.Where(r => r.VenueId == request.VenueId.Value);
        }

        // Default: bu ay
        var fromDate = request.FromDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var toDate = request.ToDate ?? fromDate.AddMonths(1).AddDays(-1);

        query = query.Where(r => r.ReservedFor >= fromDate && r.ReservedFor <= toDate);

        if (!string.IsNullOrEmpty(request.Status))
        {
            var statuses = request.Status
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Enum.TryParse<ReservationStatus>(s.Trim(), true, out var status) ? status : (ReservationStatus?)null)
                .Where(s => s.HasValue)
                .Select(s => s!.Value)
                .ToList();

            if (statuses.Count > 0)
            {
                query = query.Where(r => statuses.Contains(r.Status));
            }
        }

        var reservations = await query
            .OrderBy(r => r.ReservedFor)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // CSV oluştur
        var csv = new StringBuilder();

        // Header
        csv.AppendLine("Onay Kodu,Misafir Adı,Telefon,Email,Kişi Sayısı,Tarih/Saat,Masa,Mekan,Durum,Kapora,Özel İstekler,Oluşturulma");

        // Rows
        foreach (var r in reservations)
        {
            var row = string.Join(",",
                EscapeCsv(r.ConfirmCode),
                EscapeCsv(r.GuestName),
                EscapeCsv(r.GuestPhone),
                EscapeCsv(r.GuestEmail ?? string.Empty),
                r.PartySize,
                r.ReservedFor.ToString("yyyy-MM-dd HH:mm"),
                EscapeCsv(r.Table?.Name ?? string.Empty),
                EscapeCsv(r.Venue?.Name ?? string.Empty),
                r.Status.ToString(),
                r.DepositAmount?.ToString("F2") ?? "0",
                EscapeCsv(r.SpecialRequests ?? string.Empty),
                r.CreatedAt.ToString("yyyy-MM-dd HH:mm"));

            csv.AppendLine(row);
        }

        var content = Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"rezervasyonlar_{fromDate:yyyyMM}.csv";

        return new ExportReservationsResult
        {
            Content = content,
            FileName = fileName,
            ContentType = "text/csv; charset=utf-8"
        };
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
