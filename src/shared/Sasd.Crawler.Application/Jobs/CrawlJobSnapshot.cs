using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Jobs;

/// <summary>
/// Read model for one durable crawl job.
/// </summary>
public sealed record CrawlJobSnapshot(
    CrawlJobId Id,
    SourceId SourceId,
    CrawlJobStatus Status,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    int? ObservedDocuments,
    int? NonFatalIssues,
    int? NewlyMissingDocuments,
    string? LastError);
