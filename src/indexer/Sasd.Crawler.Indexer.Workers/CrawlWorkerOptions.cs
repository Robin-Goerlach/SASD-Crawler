namespace Sasd.Crawler.Indexer.Workers;

/// <summary>
/// Runtime tuning options for the crawl-work background loop.
/// </summary>
/// <remarks>
/// These defaults favor predictable correctness over aggressive throughput. Polling and lease durations can be
/// measured and tuned later without changing queue or scan semantics.
/// </remarks>
public sealed class CrawlWorkerOptions
{
    /// <summary>Gets or sets how long an idle worker waits before checking the durable queue again.</summary>
    public TimeSpan PollInterval { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the informational ownership horizon recorded on newly leased work.
    /// </summary>
    /// <remarks>
    /// Version 0.1 does not automatically steal expired leases. The timestamp is persisted now because later
    /// restart/recovery policy requires it and the baseline explicitly calls for LeaseUntil.
    /// </remarks>
    public TimeSpan LeaseDuration { get; init; } = TimeSpan.FromMinutes(30);

    internal void Validate()
    {
        if (PollInterval <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("Crawl worker poll interval must be positive.");
        }

        if (LeaseDuration <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("Crawl worker lease duration must be positive.");
        }
    }
}
