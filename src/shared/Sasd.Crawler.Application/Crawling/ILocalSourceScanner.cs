using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Crawling;

/// <summary>
/// UI-neutral execution boundary for one local source full scan.
/// </summary>
public interface ILocalSourceScanner
{
    /// <summary>Executes one full scan and returns its durable reconciliation outcome.</summary>
    Task<LocalSourceScanResult> RunFullScanAsync(
        SourceId sourceId,
        LocalScanOptions options,
        CancellationToken cancellationToken);
}
