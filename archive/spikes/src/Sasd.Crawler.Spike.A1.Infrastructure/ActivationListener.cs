using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sasd.Crawler.Spike.A1.Infrastructure;

/// <summary>Continuously accepts second-instance activation signals until host shutdown.</summary>
public sealed class ActivationListener(
    SingleInstanceCoordinator coordinator,
    ILogger<ActivationListener> logger) : BackgroundService
{
    public event EventHandler? ActivationRequested;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await coordinator.ListenOnceAsync(() =>
                {
                    logger.LogInformation("Second-instance activation received");
                    ActivationRequested?.Invoke(this, EventArgs.Empty);
                    return Task.CompletedTask;
                }, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (IOException exception)
            {
                logger.LogWarning(exception, "Activation pipe failed; listener will retry");
                await Task.Delay(250, stoppingToken).ConfigureAwait(false);
            }
        }
    }
}
