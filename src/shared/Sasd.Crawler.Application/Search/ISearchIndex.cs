using Sasd.Crawler.Domain.Documents;

namespace Sasd.Crawler.Application.Search;

/// <summary>
/// Rebuildable full-text search index boundary.
/// </summary>
/// <remarks>
/// Implementations may be deleted and rebuilt from durable control/document state. They must never become the sole
/// owner of source, document, crawl or deletion truth.
/// </remarks>
public interface ISearchIndex : IAsyncDisposable
{
    /// <summary>Upserts one document by stable DocumentId.</summary>
    Task UpsertAsync(SearchIndexDocument document, CancellationToken cancellationToken);

    /// <summary>Deletes one known DocumentId from the current search index.</summary>
    Task DeleteAsync(DocumentId documentId, CancellationToken cancellationToken);

    /// <summary>Commits pending mutations to durable index storage.</summary>
    Task CommitAsync(CancellationToken cancellationToken);

    /// <summary>Runs a ranked keyword search against indexed title/content fields.</summary>
    Task<SearchResultPage> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
}
