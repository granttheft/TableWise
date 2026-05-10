using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tablewise.Infrastructure.Email.Services;

namespace Tablewise.Infrastructure.HostedServices;

/// <summary>
/// Background worker. Redis queue'dan email alır, SendGridEmailService'e gönderir.
/// </summary>
public sealed class EmailWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailWorkerService> _logger;

    public EmailWorkerService(IServiceProvider serviceProvider, ILogger<EmailWorkerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailWorkerService başlatıldı");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var queueService = scope.ServiceProvider.GetRequiredService<EmailQueueService>();
                var sendGridService = scope.ServiceProvider.GetRequiredService<SendGridEmailService>();

                var request = await queueService.DequeueAsync(TimeSpan.FromSeconds(5), stoppingToken);

                if (request != null)
                {
                    _logger.LogInformation("Email işleniyor. To={To}, Template={Template}", 
                        request.To, request.TemplateName);

                    await sendGridService.SendAsync(request, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmailWorkerService hatası");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("EmailWorkerService durduruldu");
    }
}
