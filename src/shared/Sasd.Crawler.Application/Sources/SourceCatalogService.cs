using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Sources;

/// <summary>
/// Implements source configuration use cases without depending on a concrete persistence technology.
/// </summary>
public sealed class SourceCatalogService : ISourceCatalog
{
    private readonly ISourceRepository repository;
    private readonly TimeProvider timeProvider;

    /// <summary>
    /// Initializes a new source catalog service.
    /// </summary>
    public SourceCatalogService(ISourceRepository repository, TimeProvider timeProvider)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken)
        => repository.InitializeAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<SourceSummary>> GetSourcesAsync(CancellationToken cancellationToken)
    {
        var sources = await repository.ListAsync(cancellationToken).ConfigureAwait(false);
        return sources.Select(ToSummary).ToArray();
    }

    /// <inheritdoc />
    public async Task<RegisterLocalSourceResult> RegisterLocalDirectoryAsync(
        string directoryPath,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizeLocalDirectory(directoryPath);

        var existing = await repository
            .FindByCanonicalLocationAsync(normalized.CanonicalLocation, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return new RegisterLocalSourceResult(ToSummary(existing), Created: false);
        }

        var now = timeProvider.GetUtcNow();
        var candidate = SourceDefinition.CreateLocalDirectory(
            normalized.DisplayName,
            normalized.CanonicalLocation,
            now);

        // TryAdd is deliberately race-safe. The desktop product is normally single-instance, but storage code
        // should not rely on that UI invariant forever because a future CLI/service host may exist.
        if (await repository.TryAddAsync(candidate, cancellationToken).ConfigureAwait(false))
        {
            return new RegisterLocalSourceResult(ToSummary(candidate), Created: true);
        }

        existing = await repository
            .FindByCanonicalLocationAsync(normalized.CanonicalLocation, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            throw new InvalidOperationException(
                "The source was not inserted, but no existing source can be read for the canonical location.");
        }

        return new RegisterLocalSourceResult(ToSummary(existing), Created: false);
    }

    /// <inheritdoc />
    public async Task SetEnabledAsync(
        SourceId sourceId,
        bool isEnabled,
        CancellationToken cancellationToken)
    {
        var sources = await repository.ListAsync(cancellationToken).ConfigureAwait(false);
        var source = sources.SingleOrDefault(candidate => candidate.Id == sourceId)
            ?? throw new KeyNotFoundException($"Source '{sourceId}' does not exist.");

        if (!source.SetEnabled(isEnabled, timeProvider.GetUtcNow()))
        {
            return;
        }

        await repository.UpdateAsync(source, cancellationToken).ConfigureAwait(false);
    }

    private static SourceSummary ToSummary(SourceDefinition source)
        => new(
            source.Id,
            source.Kind,
            source.DisplayName,
            source.CanonicalLocation,
            GetDisplayLocation(source),
            source.IsEnabled);

    private static string GetDisplayLocation(SourceDefinition source)
    {
        if (source.Kind != SourceKind.LocalDirectory)
        {
            return source.CanonicalLocation;
        }

        return Uri.TryCreate(source.CanonicalLocation, UriKind.Absolute, out var uri) && uri.IsFile
            ? uri.LocalPath
            : source.CanonicalLocation;
    }

    private static NormalizedLocalDirectory NormalizeLocalDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("A local directory path is required.", nameof(directoryPath));
        }

        // Full-path normalization belongs in the application layer because the Domain treats canonical locations
        // as opaque values. This keeps Windows/path rules out of the core aggregate.
        var fullPath = Path.GetFullPath(directoryPath.Trim());
        if (!Path.EndsInDirectorySeparator(fullPath))
        {
            fullPath += Path.DirectorySeparatorChar;
        }

        var uri = new Uri(fullPath, UriKind.Absolute);
        if (!uri.IsFile)
        {
            throw new ArgumentException("The local source must resolve to an absolute file location.", nameof(directoryPath));
        }

        var directoryInfo = new DirectoryInfo(fullPath);
        var displayName = string.IsNullOrWhiteSpace(directoryInfo.Name)
            ? fullPath
            : directoryInfo.Name;

        return new NormalizedLocalDirectory(uri.AbsoluteUri, displayName);
    }

    private sealed record NormalizedLocalDirectory(string CanonicalLocation, string DisplayName);
}
