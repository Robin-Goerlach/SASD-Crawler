using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sasd.Crawler.Spike.A1.Core;

/// <summary>Produces harmless periodic heartbeats while honoring host cancellation.</summary>
public sealed class HeartbeatWorker(
    IHeartbeatStore store,
    IHeartbeatState state,
    ILogger<HeartbeatWorker> logger,
    TimeSpan interval) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Heartbeat worker started");
        await store.InitializeAsync(stoppingToken).ConfigureAwait(false);

        try
        {
            // Write immediately so startup evidence does not depend on the first timer tick.
            await WriteHeartbeatAsync(stoppingToken).ConfigureAwait(false);
            using var timer = new PeriodicTimer(interval);
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                if (!state.IsPaused)
                    await WriteHeartbeatAsync(stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Cancellation is the expected Generic Host shutdown path.
        }
        finally
        {
            logger.LogInformation("Heartbeat worker stopped");
        }
    }

    private async Task WriteHeartbeatAsync(CancellationToken cancellationToken)
    {
        var heartbeat = new Heartbeat(DateTimeOffset.UtcNow, "Running", Environment.ProcessId);
        await store.WriteAsync(heartbeat, cancellationToken).ConfigureAwait(false);
        state.Publish(heartbeat);
    }
}
