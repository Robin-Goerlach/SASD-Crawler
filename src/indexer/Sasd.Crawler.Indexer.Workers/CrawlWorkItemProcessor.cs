using Microsoft.Extensions.Logging;
using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Application.Jobs;
using Sasd.Crawler.Application.Search;
using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Jobs;

namespace Sasd.Crawler.Indexer.Workers;

/// <summary>
/// Executes at most one durable crawl/index work item per call.
/// </summary>
/// <remarks>
/// Discovery and document processing are separate durable stages. This keeps parser/search failures from changing
/// scan reconciliation authority and makes the pending work visible across process restarts.
/// </remarks>
public sealed class CrawlWorkItemProcessor
{
    private readonly ICrawlWorkQueue workQueue;
    private readonly ILocalSourceScanner scanner;
    private readonly IDocumentRegistry documentRegistry;
    private readonly IDocumentIndexingService indexingService;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<CrawlWorkItemProcessor> logger;

    /// <summary>Creates the durable work-item processor.</summary>
    public CrawlWorkItemProcessor(
        ICrawlWorkQueue workQueue,
        ILocalSourceScanner scanner,
        IDocumentRegistry documentRegistry,
        IDocumentIndexingService indexingService,
        TimeProvider timeProvider,
        ILogger<CrawlWorkItemProcessor> logger)
    {
        this.workQueue = workQueue ?? throw new ArgumentNullException(nameof(workQueue));
        this.scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        this.documentRegistry = documentRegistry ?? throw new ArgumentNullException(nameof(documentRegistry));
        this.indexingService = indexingService ?? throw new ArgumentNullException(nameof(indexingService));
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Leases and executes one ready durable item.</summary>
    public async Task<bool> ProcessNextAsync(
        string leaseOwner,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken)
    {
        var workItem = await workQueue
            .TryLeaseNextAsync(leaseOwner, timeProvider.GetUtcNow(), leaseDuration, cancellationToken)
            .ConfigureAwait(false);

        if (workItem is null)
        {
            return false;
        }

        try
        {
            switch (workItem.Stage)
            {
                case WorkItemStage.LocalFullScan:
                    await ProcessFullScanAsync(workItem, cancellationToken).ConfigureAwait(false);
                    break;

                case WorkItemStage.IndexDocument:
                    await ProcessIndexDocumentAsync(workItem, cancellationToken).ConfigureAwait(false);
                    break;

                case WorkItemStage.DeleteFromSearchIndex:
                    await ProcessSearchDeleteAsync(workItem, cancellationToken).ConfigureAwait(false);
                    break;

                default:
                    throw new NotSupportedException(
                        $"Work-item stage '{workItem.Stage}' is not supported by the crawl processor.");
            }

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await ReleaseAfterCancellationAsync(workItem).ConfigureAwait(false);
            throw;
        }
        catch (Exception exception)
        {
            var errorSummary = BuildErrorSummary(exception);

            await workQueue
                .MarkFailedAsync(
                    workItem,
                    errorSummary,
                    timeProvider.GetUtcNow(),
                    CancellationToken.None)
                .ConfigureAwait(false);

            logger.LogError(
                exception,
                "Crawl job {CrawlJobId} failed while executing {Stage} work item {WorkItemId}.",
                workItem.CrawlJobId,
                workItem.Stage,
                workItem.WorkItemId);

            return true;
        }
    }

    private async Task ProcessFullScanAsync(
        LeasedCrawlWorkItem workItem,
        CancellationToken cancellationToken)
    {
        var result = await scanner
            .RunFullScanAsync(
                workItem.SourceId,
                new LocalScanOptions(workItem.MaximumDepth),
                cancellationToken)
            .ConfigureAwait(false);

        var observedDocuments = await documentRegistry
            .ListDocumentsSeenInScanAsync(result.ScanRunId, cancellationToken)
            .ConfigureAwait(false);

        var followUps = new List<DocumentWorkRequest>();

        foreach (var document in observedDocuments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (indexingService.CanIndex(document))
            {
                followUps.Add(new DocumentWorkRequest(document.Id, WorkItemStage.IndexDocument));
            }
        }

        // Missing IDs originate from the authoritative reconciliation transaction itself. A partial/failed scan
        // never returns deletion IDs, so parser/index cleanup cannot accidentally turn uncertainty into deletion.
        foreach (var documentId in result.NewlyMissingDocumentIds)
        {
            followUps.Add(new DocumentWorkRequest(documentId, WorkItemStage.DeleteFromSearchIndex));
        }

        await workQueue
            .CompleteFullScanAsync(
                workItem,
                result,
                followUps,
                timeProvider.GetUtcNow(),
                cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation(
            "Discovery for crawl job {CrawlJobId} completed as {Outcome}; observed {ObservedDocuments} documents and queued {FollowUpCount} processing items.",
            workItem.CrawlJobId,
            result.Outcome,
            result.ObservedDocuments,
            followUps.Count);
    }

    private async Task ProcessIndexDocumentAsync(
        LeasedCrawlWorkItem workItem,
        CancellationToken cancellationToken)
    {
        var documentId = RequireDocumentId(workItem);
        var document = await documentRegistry
            .GetDocumentAsync(documentId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Document '{documentId}' no longer exists in the registry.");

        if (document.IsMissing || !indexingService.CanIndex(document))
        {
            // The document may have disappeared or extraction policy may have changed after the work item was
            // planned. Removing any stale index entry is safer than re-indexing obsolete content.
            await indexingService.DeleteAsync(documentId, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await indexingService.IndexAsync(document, cancellationToken).ConfigureAwait(false);
        }

        await workQueue
            .CompleteProcessingAsync(workItem, timeProvider.GetUtcNow(), cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ProcessSearchDeleteAsync(
        LeasedCrawlWorkItem workItem,
        CancellationToken cancellationToken)
    {
        var documentId = RequireDocumentId(workItem);
        await indexingService.DeleteAsync(documentId, cancellationToken).ConfigureAwait(false);

        await workQueue
            .CompleteProcessingAsync(workItem, timeProvider.GetUtcNow(), cancellationToken)
            .ConfigureAwait(false);
    }

    private static DocumentId RequireDocumentId(LeasedCrawlWorkItem workItem)
        => workItem.DocumentId
            ?? throw new InvalidDataException(
                $"Work item '{workItem.WorkItemId}' stage '{workItem.Stage}' requires a DocumentId.");

    private async Task ReleaseAfterCancellationAsync(LeasedCrawlWorkItem workItem)
    {
        try
        {
            await workQueue
                .ReleaseLeaseAsync(
                    workItem,
                    timeProvider.GetUtcNow(),
                    CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Could not release crawl work item {WorkItemId} during host shutdown.",
                workItem.WorkItemId);
        }
    }

    private static string BuildErrorSummary(Exception exception)
    {
        const int maximumLength = 1024;
        var value = $"{exception.GetType().Name}: {exception.Message}";
        return value.Length <= maximumLength ? value : value[..maximumLength];
    }
}
