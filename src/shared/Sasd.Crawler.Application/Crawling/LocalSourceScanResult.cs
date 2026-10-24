using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Documents;

namespace Sasd.Crawler.Application.Crawling;

/// <summary>
/// Summarizes one completed local source scan.
/// </summary>
public sealed record LocalSourceScanResult(
    ScanRunId ScanRunId,
    ScanRunOutcome Outcome,
    int ObservedDocuments,
    int NonFatalIssues,
    int NewlyMissingDocuments)
{
    /// <summary>Gets documents newly classified missing by this scan.</summary>
    public IReadOnlyList<DocumentId> NewlyMissingDocumentIds { get; init; } = [];
}
