using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Search;

/// <summary>
/// Immutable content payload written to the rebuildable search index.
/// </summary>
/// <remarks>
/// Lifecycle truth remains in SQLite/Document Registry. This DTO intentionally contains only data needed by the
/// derived search artifact so losing or rebuilding the Lucene index cannot lose source/document identity.
/// </remarks>
public sealed class SearchIndexDocument
{
    /// <summary>Creates a search-index document.</summary>
    public SearchIndexDocument(
        DocumentId documentId,
        SourceId sourceId,
        string canonicalLocator,
        string title,
        string content,
        string language,
        DateTimeOffset modifiedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalLocator);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        DocumentId = documentId;
        SourceId = sourceId;
        CanonicalLocator = canonicalLocator.Trim();
        Title = title.Trim();
        Content = content;
        Language = language.Trim().ToLowerInvariant();
        ModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
    }

    /// <summary>Gets the stable registry document identity.</summary>
    public DocumentId DocumentId { get; }

    /// <summary>Gets the owning configured source.</summary>
    public SourceId SourceId { get; }

    /// <summary>Gets the canonical location used to open or diagnose the original.</summary>
    public string CanonicalLocator { get; }

    /// <summary>Gets the display title/name.</summary>
    public string Title { get; }

    /// <summary>Gets the extracted searchable text.</summary>
    public string Content { get; }

    /// <summary>Gets the normalized language tag used for analyzer selection.</summary>
    public string Language { get; }

    /// <summary>Gets the original document modification timestamp.</summary>
    public DateTimeOffset ModifiedAtUtc { get; }
}
