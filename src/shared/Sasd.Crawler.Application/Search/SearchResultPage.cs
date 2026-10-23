using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Search;

/// <summary>
/// One basic search result returned by the search backend.
/// </summary>
public sealed record SearchHit(
    DocumentId DocumentId,
    SourceId SourceId,
    string Title,
    string CanonicalLocator,
    float Score);

/// <summary>
/// Page of ranked keyword-search hits.
/// </summary>
public sealed record SearchResultPage(IReadOnlyList<SearchHit> Hits, long TotalHits);
