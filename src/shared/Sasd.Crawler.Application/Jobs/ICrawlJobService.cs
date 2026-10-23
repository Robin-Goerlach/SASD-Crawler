using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Jobs;

/// <summary>
/// UI-neutral use-case boundary for requesting durable crawl jobs.
/// </summary>
public interface ICrawlJobService
{
    /// <summary>Initializes source and queue persistence needed for job requests.</summary>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Queues a full scan for one enabled local-directory source without performing the scan on the caller thread.
    /// </summary>
    Task<CrawlJobId> QueueLocalFullScanAsync(
        SourceId sourceId,
        LocalScanOptions options,
        CancellationToken cancellationToken);

    /// <summary>Reads current durable state for a previously queued crawl job.</summary>
    Task<CrawlJobSnapshot?> GetJobAsync(CrawlJobId jobId, CancellationToken cancellationToken);
}
