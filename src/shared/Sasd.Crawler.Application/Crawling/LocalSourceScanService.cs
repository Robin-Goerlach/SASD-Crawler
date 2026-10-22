using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Crawling;

/// <summary>
/// Coordinates a full local-filesystem scan, durable LastSeen updates and safe reconciliation.
/// </summary>
/// <remarks>
/// The service deliberately separates "enumeration completed" from "authoritative scan completed". Individual
/// file errors do not abort traversal, but they downgrade the run to <see cref="ScanRunOutcome.Partial"/> and
/// therefore suppress global missing reconciliation. Fatal traversal/root errors mark the run failed.
/// </remarks>
public sealed class LocalSourceScanService
{
    private readonly ISourceRepository sourceRepository;
    private readonly IDocumentRegistry documentRegistry;
    private readonly ILocalFileSystemConnector connector;
    private readonly TimeProvider timeProvider;

    /// <summary>Creates the full-scan coordinator.</summary>
    public LocalSourceScanService(
        ISourceRepository sourceRepository,
        IDocumentRegistry documentRegistry,
        ILocalFileSystemConnector connector,
        TimeProvider timeProvider)
    {
        this.sourceRepository = sourceRepository ?? throw new ArgumentNullException(nameof(sourceRepository));
        this.documentRegistry = documentRegistry ?? throw new ArgumentNullException(nameof(documentRegistry));
        this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    /// Performs one full scan of an enabled local-directory source.
    /// </summary>
    public async Task<LocalSourceScanResult> RunFullScanAsync(
        SourceId sourceId,
        LocalScanOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var source = await sourceRepository.GetByIdAsync(sourceId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Source '{sourceId}' does not exist.");

        if (!source.IsEnabled)
        {
            throw new InvalidOperationException($"Source '{sourceId}' is disabled and cannot be scanned.");
        }

        if (source.Kind != SourceKind.LocalDirectory)
        {
            throw new InvalidOperationException(
                $"Source '{sourceId}' is '{source.Kind}' and cannot be scanned by the local-filesystem connector.");
        }

        await documentRegistry.InitializeAsync(cancellationToken).ConfigureAwait(false);

        var scanRunId = await documentRegistry
            .BeginFullScanAsync(sourceId, timeProvider.GetUtcNow(), cancellationToken)
            .ConfigureAwait(false);

        var observedDocuments = 0;
        var nonFatalIssues = 0;

        try
        {
            foreach (var scanEvent in connector.Enumerate(source, options, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch (scanEvent)
                {
                    case LocalFileObserved file:
                        await documentRegistry
                            .RecordSeenAsync(
                                scanRunId,
                                sourceId,
                                file.Observation,
                                timeProvider.GetUtcNow(),
                                cancellationToken)
                            .ConfigureAwait(false);
                        observedDocuments++;
                        break;

                    case LocalScanIssueReported:
                        // LOC-005 says one unreadable item must not terminate the crawl. However, silently treating
                        // the run as complete would convert an access problem into a false deletion signal.
                        nonFatalIssues++;
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unknown local scan event type '{scanEvent.GetType().FullName}'.");
                }
            }

            var outcome = nonFatalIssues == 0
                ? ScanRunOutcome.Succeeded
                : ScanRunOutcome.Partial;

            var completion = await documentRegistry
                .CompleteFullScanAsync(
                    scanRunId,
                    sourceId,
                    outcome,
                    nonFatalIssues,
                    timeProvider.GetUtcNow(),
                    cancellationToken)
                .ConfigureAwait(false);

            return new LocalSourceScanResult(
                scanRunId,
                outcome,
                observedDocuments,
                nonFatalIssues,
                completion.NewlyMissingDocuments);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await BestEffortAbortAsync(
                    scanRunId,
                    sourceId,
                    ScanRunOutcome.Cancelled,
                    "Scan cancelled.",
                    timeProvider.GetUtcNow())
                .ConfigureAwait(false);
            throw;
        }
        catch (Exception exception)
        {
            await BestEffortAbortAsync(
                    scanRunId,
                    sourceId,
                    ScanRunOutcome.Failed,
                    BuildErrorSummary(exception),
                    timeProvider.GetUtcNow())
                .ConfigureAwait(false);
            throw;
        }
    }

    private async Task BestEffortAbortAsync(
        ScanRunId scanRunId,
        SourceId sourceId,
        ScanRunOutcome outcome,
        string errorSummary,
        DateTimeOffset completedAtUtc)
    {
        try
        {
            // The caller's token may already be cancelled. Persisting a terminal non-authoritative state is a
            // separate safety action, so it intentionally uses CancellationToken.None.
            await documentRegistry
                .AbortFullScanAsync(
                    scanRunId,
                    sourceId,
                    outcome,
                    errorSummary,
                    completedAtUtc,
                    CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch
        {
            // Never mask the original crawl/cancellation exception. A stale Running scan is fail-closed because
            // only CompleteFullScanAsync(Succeeded) can perform reconciliation. Recovery can clean stale runs later.
        }
    }

    private static string BuildErrorSummary(Exception exception)
    {
        const int maximumLength = 512;
        var message = $"{exception.GetType().Name}: {exception.Message}";
        return message.Length <= maximumLength
            ? message
            : message[..maximumLength];
    }
}
