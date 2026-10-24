using Sasd.Crawler.Domain.Documents;

namespace Sasd.Crawler.Application.Search;

/// <summary>
/// Coordinates extraction and search-index mutation for durable document records.
/// </summary>
public interface IDocumentIndexingService
{
    /// <summary>Returns whether at least one configured extractor can process the document.</summary>
    bool CanIndex(DocumentRecord document);

    /// <summary>Extracts and upserts one current document into the rebuildable search index.</summary>
    Task IndexAsync(DocumentRecord document, CancellationToken cancellationToken);

    /// <summary>Removes one document from the rebuildable search index.</summary>
    Task DeleteAsync(DocumentId documentId, CancellationToken cancellationToken);
}
