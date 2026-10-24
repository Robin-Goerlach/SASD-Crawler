using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Jobs;

/// <summary>
/// Durable queue boundary used by application services and background workers.
/// </summary>
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

    /// <summary>Atomically leases the oldest ready pending work item, or returns null when none is ready.</summary>
    Task<LeasedCrawlWorkItem?> TryLeaseNextAsync(
        string leaseOwner,
        DateTimeOffset nowUtc,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken);

    /// <summary>
    /// Completes discovery and atomically attaches any document-processing follow-up work to the same crawl job.
    /// </summary>
    Task CompleteFullScanAsync(
        LeasedCrawlWorkItem workItem,
        LocalSourceScanResult result,
        IReadOnlyCollection<DocumentWorkRequest> followUpWork,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken);

    /// <summary>
    /// Completes one document-processing stage and finalizes the job when no pending/leased work remains.
    /// </summary>
    Task CompleteProcessingAsync(
        LeasedCrawlWorkItem workItem,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken);

    /// <summary>Persists terminal failure for the currently owned work item.</summary>
    Task MarkFailedAsync(
        LeasedCrawlWorkItem workItem,
        string errorSummary,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken);

    /// <summary>Releases an owned lease back to Pending, for example during cooperative host shutdown.</summary>
    Task ReleaseLeaseAsync(
        LeasedCrawlWorkItem workItem,
        DateTimeOffset nextAttemptUtc,
        CancellationToken cancellationToken);

    /// <summary>Returns one job snapshot for diagnostics/UI status, or null when the job is unknown.</summary>
    Task<CrawlJobSnapshot?> GetJobAsync(CrawlJobId jobId, CancellationToken cancellationToken);
}
