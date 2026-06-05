using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Infrastructure.HostedServices;

/// <summary>
/// WhatsApp rezervasyon hatırlatma servisi.
/// Her saat çalışır; yarın rezervasyonu olan, WhatsApp aktif mekanlar için
/// henüz gönderilmemiş Reminder mesajlarını gönderir.
/// </summary>
public sealed class WhatsAppReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WhatsAppReminderService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    /// <summary>
    /// WhatsAppReminderService constructor.
    /// </summary>
    public WhatsAppReminderService(
        IServiceScopeFactory scopeFactory,
        ILogger<WhatsAppReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WhatsAppReminderService başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRemindersAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsAppReminderService hatası.");
            }

            await Task.Delay(CheckInterval, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task SendRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var orchestrator = scope.ServiceProvider.GetRequiredService<IWhatsAppOrchestrator>();

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var tomorrowEnd = tomorrow.AddDays(1);

        // Yarın rezervasyonu olan, onaylanmış rezervasyonlar
        var reservations = await unitOfWork.Reservations
            .Query()
            .IgnoreQueryFilters()
            .Include(r => r.Venue)
            .Where(r =>
                !r.IsDeleted &&
                r.Status == ReservationStatus.Confirmed &&
                r.ReservedFor >= tomorrow &&
                r.ReservedFor < tomorrowEnd &&
                r.Venue != null &&
                r.Venue.WhatsAppEnabled &&
                !r.IsDeleted)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (reservations.Count == 0) return;

        // Daha önce Reminder gönderilen rezervasyon ID'leri
        var reservationIds = reservations.Select(r => r.Id).ToList();
        var alreadySent = await unitOfWork.WhatsAppMessages
            .Query()
            .IgnoreQueryFilters()
            .Where(m =>
                m.ReservationId.HasValue &&
                reservationIds.Contains(m.ReservationId.Value) &&
                m.Template == WhatsAppMessageTemplate.Reminder &&
                m.Status != WhatsAppMessageStatus.Failed)
            .Select(m => m.ReservationId!.Value)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var toSend = reservations.Where(r => !alreadySent.Contains(r.Id)).ToList();

        _logger.LogInformation(
            "WhatsApp Reminder: {Total} rezervasyon kontrolü, {ToSend} gönderilecek.",
            reservations.Count, toSend.Count);

        foreach (var reservation in toSend)
        {
            await orchestrator.SendReminderAsync(
                reservation,
                reservation.Venue!.Name,
                reservation.Venue.Address,
                venueWhatsAppEnabled: true,
                waNotify: reservation.Venue.WaNotifyReminder,
                ct).ConfigureAwait(false);
        }
    }
}
