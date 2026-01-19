using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XYZ.Application.Common.Interfaces;

namespace XYZ.API.HostedServices;

public sealed class OverduePaymentsHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<OverduePaymentsHostedService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<OverduePaymentsHostedService> _logger = logger;

    private const int RunHour = 0;
    private const int RunMinute = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OverduePaymentsHostedService started. Scheduled daily at {Hour:D2}:{Minute:D2}.", RunHour, RunMinute);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nowUtc = DateTimeOffset.UtcNow;
                var tz = GetIstanbulTimeZone();

                var nowLocal = TimeZoneInfo.ConvertTime(nowUtc, tz);

                var nextRunLocal = new DateTimeOffset(
                    year: nowLocal.Year,
                    month: nowLocal.Month,
                    day: nowLocal.Day,
                    hour: RunHour,
                    minute: RunMinute,
                    second: 0,
                    offset: nowLocal.Offset);

                if (nowLocal >= nextRunLocal)
                {
                    nextRunLocal = nextRunLocal.AddDays(1);
                }

                var delay = nextRunLocal - nowLocal;
                if (delay < TimeSpan.Zero)
                {
                    delay = TimeSpan.FromMinutes(1);
                }

                _logger.LogDebug("Next overdue job run at {NextRunLocal} (in {Delay}).", nextRunLocal, delay);

                await Task.Delay(delay, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IOverduePaymentService>();

                var affected = await service.MarkOverdueAsync(stoppingToken);
                _logger.LogInformation("Overdue payments job completed. Updated rows: {Affected}.", affected);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Overdue payments job failed.");

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("OverduePaymentsHostedService stopped.");
    }

    private static TimeZoneInfo GetIstanbulTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
        catch
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
    }
}
