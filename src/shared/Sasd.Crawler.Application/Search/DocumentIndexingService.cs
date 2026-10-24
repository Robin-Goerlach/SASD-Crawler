using Sasd.Crawler.Application.Extraction;
using Sasd.Crawler.Domain.Documents;

namespace Sasd.Crawler.Application.Search;

/// <summary>
/// Application service that turns extracted document content into search-index mutations.
/// </summary>
/// <remarks>
/// Extractor selection lives outside Lucene so future Tika/native/OCR implementations can be changed without
/// changing the search backend. Each successful mutation is explicitly committed in this correctness-first slice;
/// batching is a later optimization and must preserve the same observable durability semantics.
/// </remarks>
public sealed class DocumentIndexingService : IDocumentIndexingService
{
    private readonly IContentExtractor[] extractors;
    private readonly ISearchIndex searchIndex;

    /// <summary>Creates the indexing service from the configured extraction pipeline and search backend.</summary>
    public DocumentIndexingService(
        IEnumerable<IContentExtractor> extractors,
        ISearchIndex searchIndex)
    {
        ArgumentNullException.ThrowIfNull(extractors);
        this.searchIndex = searchIndex ?? throw new ArgumentNullException(nameof(searchIndex));

        this.extractors = extractors.ToArray();
        if (this.extractors.Length == 0)
        {
            throw new ArgumentException("At least one content extractor must be configured.", nameof(extractors));
        }
    }

    /// <inheritdoc />
    public bool CanIndex(DocumentRecord document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return extractors.Any(extractor => extractor.CanExtract(document));
    }

    /// <inheritdoc />
    public async Task IndexAsync(DocumentRecord document, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        var extractor = extractors.FirstOrDefault(candidate => candidate.CanExtract(document))
            ?? throw new NotSupportedException(
                $"No configured content extractor supports '{document.CanonicalLocator}'.");

        var extracted = await extractor
            .ExtractAsync(document, cancellationToken)
            .ConfigureAwait(false);

        var title = string.IsNullOrWhiteSpace(extracted.Title)
            ? document.DisplayName
            : extracted.Title.Trim();

        await searchIndex
            .UpsertAsync(
                new SearchIndexDocument(
                    document.Id,
                    document.SourceId,
                    document.CanonicalLocator,
                    title,
                    extracted.Text,
                    extracted.Language,
                    document.ModifiedAtUtc),
                cancellationToken)
            .ConfigureAwait(false);

        await searchIndex.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(DocumentId documentId, CancellationToken cancellationToken)
    {
        await searchIndex.DeleteAsync(documentId, cancellationToken).ConfigureAwait(false);
        await searchIndex.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
