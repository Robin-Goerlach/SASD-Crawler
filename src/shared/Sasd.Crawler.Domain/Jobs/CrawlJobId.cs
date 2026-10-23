namespace Sasd.Crawler.Domain.Jobs;

/// <summary>
/// Identifies one user- or scheduler-requested crawl operation independently from its execution attempts.
/// </summary>
/// <remarks>
/// A job is the durable user-visible unit of work. One job may later contain several work items or retries, so it
/// deliberately has a different identity from <c>WorkItemId</c> and <c>ScanRunId</c>.
/// </remarks>
public readonly record struct CrawlJobId
{
    /// <summary>Creates a crawl-job identifier from an existing persisted value.</summary>
    public CrawlJobId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A crawl-job identifier must not be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Gets the underlying globally unique identifier.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new crawl-job identifier.</summary>
    public static CrawlJobId New() => new(Guid.NewGuid());

    /// <summary>Parses a persisted crawl-job identifier.</summary>
    public static CrawlJobId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc />
    public override string ToString() => Value.ToString("D");
}
