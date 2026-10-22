using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Domain.Documents;

/// <summary>
/// Represents durable registry metadata for one logical document location.
/// </summary>
/// <remarks>
/// The registry is control/lifecycle truth. Search-engine fields and extracted text intentionally do not live in
/// this aggregate because Lucene remains rebuildable derived state.
/// </remarks>
public sealed class DocumentRecord
{
    private DocumentRecord(
        DocumentId id,
        SourceId sourceId,
        string canonicalLocator,
        string displayName,
        long fileSize,
        DateTimeOffset? createdAtUtc,
        DateTimeOffset modifiedAtUtc,
        DateTimeOffset lastSeenAtUtc,
        ScanRunId lastSeenScanRunId,
        DateTimeOffset? missingSinceUtc)
    {
        if (string.IsNullOrWhiteSpace(canonicalLocator))
        {
            throw new ArgumentException("A canonical document locator is required.", nameof(canonicalLocator));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("A document display name is required.", nameof(displayName));
        }

        if (fileSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fileSize), "File size must not be negative.");
        }

        Id = id;
        SourceId = sourceId;
        CanonicalLocator = canonicalLocator.Trim();
        DisplayName = displayName.Trim();
        FileSize = fileSize;
        CreatedAtUtc = createdAtUtc;
        ModifiedAtUtc = modifiedAtUtc;
        LastSeenAtUtc = lastSeenAtUtc;
        LastSeenScanRunId = lastSeenScanRunId;
        MissingSinceUtc = missingSinceUtc;
    }

    /// <summary>Gets the stable internal document identifier.</summary>
    public DocumentId Id { get; }

    /// <summary>Gets the source that owns this document location.</summary>
    public SourceId SourceId { get; }

    /// <summary>Gets the canonical URI/locator used to find this location again.</summary>
    public string CanonicalLocator { get; }

    /// <summary>Gets the file name shown to the user before richer metadata extraction exists.</summary>
    public string DisplayName { get; }

    /// <summary>Gets the last successfully observed file size in bytes.</summary>
    public long FileSize { get; }

    /// <summary>Gets the filesystem creation timestamp when it was available.</summary>
    public DateTimeOffset? CreatedAtUtc { get; }

    /// <summary>Gets the last successfully observed filesystem modification timestamp.</summary>
    public DateTimeOffset ModifiedAtUtc { get; }

    /// <summary>Gets the most recent confirmed observation time.</summary>
    public DateTimeOffset LastSeenAtUtc { get; }

    /// <summary>Gets the scan run that most recently confirmed this document.</summary>
    public ScanRunId LastSeenScanRunId { get; }

    /// <summary>
    /// Gets when an authoritative scan first stopped seeing this document, or <see langword="null"/> while current.
    /// </summary>
    /// <remarks>
    /// This is deliberately a registry reconciliation marker, not the later multi-state Availability model for
    /// removable/offline media.
    /// </remarks>
    public DateTimeOffset? MissingSinceUtc { get; }

    /// <summary>Gets whether authoritative reconciliation currently considers the document missing.</summary>
    public bool IsMissing => MissingSinceUtc.HasValue;

    /// <summary>Rehydrates a document record from durable storage.</summary>
    public static DocumentRecord Restore(
        DocumentId id,
        SourceId sourceId,
        string canonicalLocator,
        string displayName,
        long fileSize,
        DateTimeOffset? createdAtUtc,
        DateTimeOffset modifiedAtUtc,
        DateTimeOffset lastSeenAtUtc,
        ScanRunId lastSeenScanRunId,
        DateTimeOffset? missingSinceUtc)
        => new(
            id,
            sourceId,
            canonicalLocator,
            displayName,
            fileSize,
            createdAtUtc,
            modifiedAtUtc,
            lastSeenAtUtc,
            lastSeenScanRunId,
            missingSinceUtc);
}
