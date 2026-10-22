namespace Sasd.Crawler.Domain.Crawling;

/// <summary>
/// Describes the terminal or active state of a source scan.
/// </summary>
public enum ScanRunOutcome
{
    /// <summary>The scan has started but has not reached a terminal state.</summary>
    Running = 1,

    /// <summary>The complete configured scope was observed without scan issues and may reconcile missing items.</summary>
    Succeeded = 2,

    /// <summary>The crawl continued after one or more item-level issues; global reconciliation is forbidden.</summary>
    Partial = 3,

    /// <summary>The scan could not enumerate its configured scope because of a fatal failure.</summary>
    Failed = 4,

    /// <summary>The scan was explicitly cancelled before authoritative completion.</summary>
    Cancelled = 5,
}
