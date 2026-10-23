namespace Sasd.Crawler.Domain.Jobs;

/// <summary>
/// Describes the durable lifecycle of a crawl job.
/// </summary>
public enum CrawlJobStatus
{
    /// <summary>The job is waiting for a worker.</summary>
    Queued = 1,

    /// <summary>A worker currently owns the job's active work item.</summary>
    Running = 2,

    /// <summary>The job completed with an authoritative successful scan.</summary>
    Succeeded = 3,

    /// <summary>The job completed but at least one scan issue made the result non-authoritative.</summary>
    Partial = 4,

    /// <summary>The active work item failed.</summary>
    Failed = 5,

    /// <summary>The job was explicitly cancelled.</summary>
    Cancelled = 6,
}
