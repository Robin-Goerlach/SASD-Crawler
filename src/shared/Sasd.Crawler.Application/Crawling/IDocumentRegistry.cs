using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Crawling;

/// <summary>
/// Durable document/scan registry port owned by the indexer side of the application boundary.
/// </summary>
public interface IDocumentRegistry
{
    /// <summary>Ensures document-registry schema/state is ready.</summary>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>Starts a new full scan in a non-authoritative running state.</summary>
    Task<ScanRunId> BeginFullScanAsync(
        SourceId sourceId,
        DateTimeOffset startedAtUtc,
        CancellationToken cancellationToken);

    /// <summary>
    /// Upserts an observed file and binds its LastSeen marker to the active scan.
    /// </summary>
    Task RecordSeenAsync(
        ScanRunId scanRunId,
        SourceId sourceId,
        FileObservation observation,
        DateTimeOffset observedAtUtc,
        CancellationToken cancellationToken);

    /// <summary>
    /// Completes a scan. Missing-document reconciliation is permitted only for <see cref="ScanRunOutcome.Succeeded"/>.
    /// </summary>
    /// <param name="outcome">Only <see cref="ScanRunOutcome.Succeeded"/> or <see cref="ScanRunOutcome.Partial"/>.</param>
    /// <returns>Counts describing the atomic completion/reconciliation step.</returns>
    Task<ScanCompletionResult> CompleteFullScanAsync(
        ScanRunId scanRunId,
        SourceId sourceId,
        ScanRunOutcome outcome,
        int issueCount,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken);

    /// <summary>
    /// Marks a running scan failed or cancelled. This operation never performs missing reconciliation.
    /// </summary>
    Task AbortFullScanAsync(
        ScanRunId scanRunId,
        SourceId sourceId,
        ScanRunOutcome outcome,
        int issueCount,
        string? errorSummary,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken);

    /// <summary>Returns durable documents known for one source.</summary>
    Task<IReadOnlyList<DocumentRecord>> ListDocumentsAsync(
        SourceId sourceId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Describes the durable effects of completing a full scan.
/// </summary>
public sealed record ScanCompletionResult(int NewlyMissingDocuments);
