using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Sources;

/// <summary>
/// UI-neutral use-case boundary for configuring and inspecting crawl sources.
/// </summary>
/// <remarks>
/// WinForms depends on this contract rather than SQLite or connector implementations. The same contract can later
/// be exposed by a CLI or an out-of-process indexer adapter without moving source-management rules into the UI.
/// </remarks>
public interface ISourceCatalog
{
    /// <summary>Initializes the underlying durable source catalog.</summary>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>Gets the currently configured sources.</summary>
    Task<IReadOnlyList<SourceSummary>> GetSourcesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Registers a local directory using a canonical absolute file URI as durable identity.
    /// </summary>
    Task<RegisterLocalSourceResult> RegisterLocalDirectoryAsync(
        string directoryPath,
        CancellationToken cancellationToken);

    /// <summary>Enables or disables an existing source.</summary>
    Task SetEnabledAsync(SourceId sourceId, bool isEnabled, CancellationToken cancellationToken);
}
