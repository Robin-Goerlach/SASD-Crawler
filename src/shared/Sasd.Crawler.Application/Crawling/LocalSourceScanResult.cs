using Sasd.Crawler.Domain.Crawling;

namespace Sasd.Crawler.Application.Crawling;

/// <summary>
/// Summarizes one completed local source scan.
/// </summary>
public sealed record LocalSourceScanResult(
    ScanRunId ScanRunId,
    ScanRunOutcome Outcome,
    int ObservedDocuments,
    int NonFatalIssues,
    int NewlyMissingDocuments);
