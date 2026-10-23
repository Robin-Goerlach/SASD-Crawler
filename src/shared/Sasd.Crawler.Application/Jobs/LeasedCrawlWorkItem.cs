using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Jobs;

/// <summary>
/// Represents one queue item currently leased to a background worker.
/// </summary>
/// <remarks>
/// Lease ownership is carried through completion/failure calls so a stale worker cannot finalize work it no longer
/// owns if stronger lease-recovery semantics are added later.
/// </remarks>
public sealed record LeasedCrawlWorkItem(
    WorkItemId WorkItemId,
    CrawlJobId CrawlJobId,
    SourceId SourceId,
    WorkItemStage Stage,
    int? MaximumDepth,
    int Attempt,
    string LeaseOwner,
    DateTimeOffset LeaseUntilUtc);
