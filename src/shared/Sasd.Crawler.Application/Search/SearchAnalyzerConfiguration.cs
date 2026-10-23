namespace Sasd.Crawler.Application.Search;

/// <summary>
/// Versioned analyzer configuration shared by indexing and querying.
/// </summary>
/// <remarks>
/// Search analysis is part of index compatibility. The same configuration version and stop-word sets must be used
/// when documents are indexed and when user queries are analyzed. A future settings UI may persist customized
/// lists, but the search backend must never silently change them because that would require a controlled reindex.
/// </remarks>
public sealed class SearchAnalyzerConfiguration
{
    private static readonly string[] DefaultGerman =
    [
        "der", "die", "das", "den", "dem", "des",
        "ein", "eine", "einer", "eines", "einem", "einen",
        "und", "oder", "ist", "sind", "im", "in", "von", "zu", "mit", "für", "auf", "an",
    ];

    private static readonly string[] DefaultEnglish =
    [
        "the", "a", "an", "and", "or", "is", "are", "in", "of", "to", "with", "for", "on", "at",
    ];

    private readonly string[] germanStopWords;
    private readonly string[] englishStopWords;

    /// <summary>Creates an explicit analyzer configuration.</summary>
    /// <param name="version">
    /// Stable configuration version persisted alongside future index metadata. Changing stop words must use a new
    /// version so callers can detect that a reindex is required.
    /// </param>
    /// <param name="germanStopWords">German words excluded from analyzed title/content fields.</param>
    /// <param name="englishStopWords">English words excluded from analyzed title/content fields.</param>
    public SearchAnalyzerConfiguration(
        string version,
        IEnumerable<string> germanStopWords,
        IEnumerable<string> englishStopWords)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentNullException.ThrowIfNull(germanStopWords);
        ArgumentNullException.ThrowIfNull(englishStopWords);

        Version = version.Trim();
        this.germanStopWords = Normalize(germanStopWords, nameof(germanStopWords));
        this.englishStopWords = Normalize(englishStopWords, nameof(englishStopWords));
    }

    /// <summary>Gets the stable analyzer configuration version.</summary>
    public string Version { get; }

    /// <summary>Gets normalized German stop words.</summary>
    public IReadOnlyList<string> GermanStopWords => germanStopWords;

    /// <summary>Gets normalized English stop words.</summary>
    public IReadOnlyList<string> EnglishStopWords => englishStopWords;

    /// <summary>
    /// Creates the conservative built-in DE/EN configuration.
    /// </summary>
    /// <remarks>
    /// The default deliberately excludes high-frequency function words such as <c>der/die/das</c> but does not
    /// exclude potentially meaningful words such as <c>ja</c> or <c>nein</c>. Administrators can later provide an
    /// explicit customized configuration rather than relying on an unchangeable hard-coded policy.
    /// </remarks>
    public static SearchAnalyzerConfiguration CreateDefault()
        => new("default-2026-10-v1", DefaultGerman, DefaultEnglish);

    private static string[] Normalize(IEnumerable<string> words, string parameterName)
    {
        var result = words
            .Select(word =>
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    throw new ArgumentException("Stop-word entries must not be empty.", parameterName);
                }

                return word.Trim().ToLowerInvariant();
            })
            .Distinct(StringComparer.Ordinal)
            .OrderBy(word => word, StringComparer.Ordinal)
            .ToArray();

        return result;
    }
}
