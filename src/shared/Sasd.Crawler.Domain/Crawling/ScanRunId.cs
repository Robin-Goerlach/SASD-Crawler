namespace Sasd.Crawler.Domain.Crawling;

/// <summary>
/// Identifies one complete attempt to enumerate a source.
/// </summary>
/// <remarks>
/// Reconciliation uses scan-run identity rather than timestamps. A document is considered observed by a scan only
/// when its <c>LastSeenScanRunId</c> equals that run. This avoids ambiguous timestamp comparisons and makes the
/// authoritative-scan rule explicit.
/// </remarks>
public readonly record struct ScanRunId
{
    /// <summary>Creates a scan-run identifier from an existing persisted value.</summary>
    public ScanRunId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A scan-run identifier must not be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Gets the underlying globally unique identifier.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new scan-run identifier.</summary>
    public static ScanRunId New() => new(Guid.NewGuid());

    /// <summary>Parses a persisted scan-run identifier.</summary>
    public static ScanRunId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc />
    public override string ToString() => Value.ToString("D");
}
