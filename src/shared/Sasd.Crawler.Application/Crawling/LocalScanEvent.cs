namespace Sasd.Crawler.Application.Crawling;

/// <summary>
/// Base type for streaming local-filesystem scan events.
/// </summary>
/// <remarks>
/// A connector returns item-level problems as events rather than throwing so the crawl can continue as required by
/// LOC-005. The orchestration layer nevertheless treats a run containing any issue as non-authoritative, thereby
/// preventing false missing-document reconciliation.
/// </remarks>
public abstract record LocalScanEvent
{
    private protected LocalScanEvent()
    {
    }
}

/// <summary>Reports one successfully observed file.</summary>
public sealed record LocalFileObserved(FileObservation Observation) : LocalScanEvent;

/// <summary>
/// Reports a non-fatal item-level filesystem problem while allowing enumeration to continue.
/// </summary>
public sealed record LocalScanIssueReported(string Path, string Message) : LocalScanEvent;
