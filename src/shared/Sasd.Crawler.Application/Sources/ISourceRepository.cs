using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Sources;

/// <summary>
/// Persistence port for durable source definitions.
/// </summary>
/// <remarks>
/// The application layer owns this abstraction; concrete storage belongs to the indexer side of ADR-0017.
/// The interface is intentionally small so SQLite can be replaced without leaking SQL into use cases.
/// </remarks>
public interface ISourceRepository
{
    /// <summary>Ensures the durable store is ready for source operations.</summary>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>Returns all configured sources in stable presentation order.</summary>
    Task<IReadOnlyList<SourceDefinition>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Returns one source by stable identity, or <see langword="null"/> when it no longer exists.</summary>
    Task<SourceDefinition?> GetByIdAsync(SourceId sourceId, CancellationToken cancellationToken);

    /// <summary>Finds a source by its canonical source-type-specific location.</summary>
    Task<SourceDefinition?> FindByCanonicalLocationAsync(
        string canonicalLocation,
        CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to persist a new source.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when inserted; <see langword="false"/> when a source with the same canonical
    /// location already exists.
    /// </returns>
    Task<bool> TryAddAsync(SourceDefinition source, CancellationToken cancellationToken);

    /// <summary>Persists mutable state of an existing source.</summary>
    Task UpdateAsync(SourceDefinition source, CancellationToken cancellationToken);
}
