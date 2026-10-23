using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Jobs;

/// <summary>
/// Durable queue boundary used by application services and background workers.
/// </summary>
/// <remarks>
/// The queue owns persistence and lease transitions but does not perform filesystem I/O itself. That separation
/// keeps job scheduling independent from the concrete connector and makes restart/error behavior testable.
/// </remarks>
public interface ICrawlWorkQueue
{
    /// <summary>Ensures durable queue schema/state is ready.</summary>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>Atomically creates one crawl job and its initial local-full-scan work item.</summary>
    Task<CrawlJobId> EnqueueLocalFullScanAsync(
        SourceId sourceId,
        LocalScanOptions options,
        DateTimeOffset requestedAtUtc,
        CancellationToken cancellationToken);

    /// <summary>
    /// Atomically leases the oldest ready pending work item, or returns <see langword="null"/> when none is ready.
    /// </summary>
    Task<LeasedCrawlWorkItem?> TryLeaseNextAsync(
        string leaseOwner,
        DateTimeOffset nowUtc,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists successful/partial scan output and finalizes the owned work item.
    /// </summary>
    Task MarkCompletedAsync(
        LeasedCrawlWorkItem workItem,
        LocalSourceScanResult result,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken);

    /// <summary>Persists terminal failure for the currently owned work item.</summary>
    Task MarkFailedAsync(
        LeasedCrawlWorkItem workItem,
        string errorSummary,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken);

    /// <summary>
    /// Releases an owned lease back to Pending, for example when the desktop host shuts down cooperatively.
    /// </summary>
    Task ReleaseLeaseAsync(
        LeasedCrawlWorkItem workItem,
        DateTimeOffset nextAttemptUtc,
        CancellationToken cancellationToken);

    /// <summary>Returns one job snapshot for diagnostics/UI status, or null when the job is unknown.</summary>
    Task<CrawlJobSnapshot?> GetJobAsync(CrawlJobId jobId, CancellationToken cancellationToken);
}
