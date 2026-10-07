namespace Sasd.Crawler.Spike.A2.Search;

public interface ISearchIndex : IAsyncDisposable
{
    Task UpsertAsync(SearchDocument document, CancellationToken cancellationToken = default);
    Task DeleteAsync(string documentId, CancellationToken cancellationToken = default);
    Task<SearchResultPage> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
}

public sealed record SearchDocument(
    string Id,
    string Title,
    string Content,
    string Language,
    string Category,
    DateTimeOffset ModifiedAt);

public sealed record SearchRequest(string Query, int Limit = 20, string? CategoryFilter = null);

public sealed record SearchHit(string Id, string Title, string Snippet, float Score);

public sealed record SearchResultPage(
    IReadOnlyList<SearchHit> Hits,
    IReadOnlyDictionary<string, int> CategoryFacets,
    long TotalHits);
