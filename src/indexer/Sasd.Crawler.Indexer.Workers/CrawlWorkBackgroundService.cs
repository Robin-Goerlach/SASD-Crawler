using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sasd.Crawler.Application.Jobs;

namespace Sasd.Crawler.Indexer.Workers;

/// <summary>
/// Generic-Host background service that drains durable crawl work without blocking the WinForms UI thread.
/// </summary>
public sealed class CrawlWorkBackgroundService : BackgroundService
{
    private readonly ICrawlWorkQueue workQueue;
    private readonly CrawlWorkItemProcessor processor;
    private readonly TimeProvider timeProvider;
    private readonly CrawlWorkerOptions options;
    private readonly ILogger<CrawlWorkBackgroundService> logger;
    private readonly string leaseOwner;

    /// <summary>Creates the crawl-work background loop.</summary>
    public CrawlWorkBackgroundService(
        ICrawlWorkQueue workQueue,
        CrawlWorkItemProcessor processor,
        TimeProvider timeProvider,
        CrawlWorkerOptions options,
        ILogger<CrawlWorkBackgroundService> logger)
    {
        this.workQueue = workQueue ?? throw new ArgumentNullException(nameof(workQueue));
        this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        options.Validate();

        // The random suffix distinguishes successive process lifetimes even if Windows reuses a process ID.
        leaseOwner = $"desktop-{Environment.ProcessId}-{Guid.NewGuid():N}";
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await workQueue.InitializeAsync(stoppingToken).ConfigureAwait(false);

        logger.LogInformation("Crawl background worker {LeaseOwner} started.", leaseOwner);

        while (!stoppingToken.IsCancellationRequested)
        {
            var processed = await processor
                .ProcessNextAsync(leaseOwner, options.LeaseDuration, stoppingToken)
                .ConfigureAwait(false);

            if (processed)
            {
                // Immediately look for the next durable item. This is intentionally single-consumer for the first
                // production milestone; controlled parallelism can be measured and added later.
                continue;
            }

            await Task.Delay(options.PollInterval, timeProvider, stoppingToken).ConfigureAwait(false);
        }
    }
}
