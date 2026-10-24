using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Jobs;

/// <summary>
/// Represents one queue item currently leased to a background worker.
/// </summary>
/// <remarks>
/// Document-processing stages carry only stable identity. Paths/content are re-read from the durable Document
/// Registry so queue payloads cannot become a competing lifecycle source of truth.
/// </remarks>
public sealed record LeasedCrawlWorkItem(
    WorkItemId WorkItemId,
    CrawlJobId CrawlJobId,
    SourceId SourceId,
    WorkItemStage Stage,
    DocumentId? DocumentId,
    int? MaximumDepth,
    int Attempt,
    string LeaseOwner,
    DateTimeOffset LeaseUntilUtc);
