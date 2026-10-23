using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Search;

/// <summary>
/// Basic 0.1 keyword-search request.
/// </summary>
public sealed record SearchRequest
{
    /// <summary>Creates a keyword-search request.</summary>
    public SearchRequest(string queryText, int limit = 50, SourceId? sourceId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queryText);
        if (limit is < 1 or > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Search limit must be between 1 and 1000.");
        }

        QueryText = queryText.Trim();
        Limit = limit;
        SourceId = sourceId;
    }

    /// <summary>Gets the user query text.</summary>
    public string QueryText { get; }

    /// <summary>Gets the maximum number of returned hits.</summary>
    public int Limit { get; }

    /// <summary>Gets an optional exact SourceId filter.</summary>
    public SourceId? SourceId { get; }
}
