using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Crawling;

/// <summary>
/// Durable document/scan registry port owned by the indexer side of the application boundary.
/// </summary>
public interface IDocumentRegistry
{
    Task InitializeAsync(CancellationToken cancellationToken);

    Task<ScanRunId> BeginFullScanAsync(
        SourceId sourceId,
        DateTimeOffset startedAtUtc,
        CancellationToken cancellationToken);

    Task RecordSeenAsync(
        ScanRunId scanRunId,
        SourceId sourceId,
        FileObservation observation,
        DateTimeOffset observedAtUtc,
        CancellationToken cancellationToken);

    Task<ScanCompletionResult> CompleteFullScanAsync(
        ScanRunId scanRunId,
        SourceId sourceId,
        ScanRunOutcome outcome,
        int issueCount,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken);

    Task AbortFullScanAsync(
        ScanRunId scanRunId,
        SourceId sourceId,
        ScanRunOutcome outcome,
        int issueCount,
        string? errorSummary,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken);

    /// <summary>Returns one durable document by identity, or null when it no longer exists.</summary>
    Task<DocumentRecord?> GetDocumentAsync(
        DocumentId documentId,
        CancellationToken cancellationToken);

    /// <summary>Returns documents observed by one specific scan run.</summary>
    Task<IReadOnlyList<DocumentRecord>> ListDocumentsSeenInScanAsync(
        ScanRunId scanRunId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentRecord>> ListDocumentsAsync(
        SourceId sourceId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Describes the durable effects of completing a full scan.
/// </summary>
public sealed record ScanCompletionResult(int NewlyMissingDocuments)
{
    /// <summary>Gets stable IDs newly classified missing by this authoritative scan.</summary>
    public IReadOnlyList<DocumentId> NewlyMissingDocumentIds { get; init; } = [];
}
