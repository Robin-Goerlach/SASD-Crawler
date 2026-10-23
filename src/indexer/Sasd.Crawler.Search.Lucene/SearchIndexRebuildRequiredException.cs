namespace Sasd.Crawler.Search.Lucene;

/// <summary>
/// Indicates that an existing Lucene index cannot be reused with the requested analyzer configuration.
/// </summary>
/// <remarks>
/// The search index is rebuildable derived state. Callers should rebuild it from durable Document Registry/content
/// state rather than trying to mix analyzer/stop-word generations inside one index.
/// </remarks>
public sealed class SearchIndexRebuildRequiredException : InvalidOperationException
{
    /// <summary>Creates an analyzer compatibility failure.</summary>
    public SearchIndexRebuildRequiredException(
        string? persistedAnalyzerVersion,
        string requiredAnalyzerVersion)
        : base(
            persistedAnalyzerVersion is null
                ? $"The existing search index has no SASD analyzer version and must be rebuilt for '{requiredAnalyzerVersion}'."
                : $"The search index analyzer version '{persistedAnalyzerVersion}' does not match required version '{requiredAnalyzerVersion}'. Rebuild is required.")
    {
        PersistedAnalyzerVersion = persistedAnalyzerVersion;
        RequiredAnalyzerVersion = requiredAnalyzerVersion;
    }

    /// <summary>Gets the analyzer version found in the index, or null for an unversioned non-empty index.</summary>
    public string? PersistedAnalyzerVersion { get; }

    /// <summary>Gets the analyzer version required by the current application configuration.</summary>
    public string RequiredAnalyzerVersion { get; }
}
