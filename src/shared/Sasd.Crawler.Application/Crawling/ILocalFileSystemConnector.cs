using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Crawling;

/// <summary>
/// Enumerates local-file-system source contents without owning registry or search-index writes.
/// </summary>
/// <remarks>
/// .NET exposes directory enumeration as synchronous streaming APIs. Implementations therefore stream and bound
/// memory, but callers must execute full scans from the indexer/background execution path rather than the WinForms
/// UI thread.
/// </remarks>
public interface ILocalFileSystemConnector
{
    /// <summary>
    /// Streams observed files and non-fatal item-level issues for the configured local source.
    /// </summary>
    IEnumerable<LocalScanEvent> Enumerate(
        SourceDefinition source,
        LocalScanOptions options,
        CancellationToken cancellationToken);
}
