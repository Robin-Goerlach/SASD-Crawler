using Microsoft.Extensions.Logging;
using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Application.Jobs;
using Sasd.Crawler.Domain.Jobs;

namespace Sasd.Crawler.Indexer.Workers;

/// <summary>
/// Executes at most one durable crawl work item per call.
/// </summary>
/// <remarks>
/// Separating one-item processing from the infinite BackgroundService loop makes ownership transitions and
/// cancellation behavior unit-testable without running a timed host loop.
/// </remarks>
public sealed class CrawlWorkItemProcessor
{
    private readonly ICrawlWorkQueue workQueue;
    private readonly ILocalSourceScanner scanner;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<CrawlWorkItemProcessor> logger;

    /// <summary>Creates the durable work-item processor.</summary>
    public CrawlWorkItemProcessor(
        ICrawlWorkQueue workQueue,
        ILocalSourceScanner scanner,
        TimeProvider timeProvider,
        ILogger<CrawlWorkItemProcessor> logger)
    {
        this.workQueue = workQueue ?? throw new ArgumentNullException(nameof(workQueue));
        this.scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Leases and executes one ready item.
    /// </summary>
    /// <returns><see langword="true"/> when an item was leased; otherwise <see langword="false"/>.</returns>
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
            if (workItem.Stage != WorkItemStage.LocalFullScan)
            {
                throw new NotSupportedException(
                    $"Work-item stage '{workItem.Stage}' is not supported by the local crawl processor.");
            }

            var result = await scanner
                .RunFullScanAsync(
                    workItem.SourceId,
                    new LocalScanOptions(workItem.MaximumDepth),
                    cancellationToken)
                .ConfigureAwait(false);

            await workQueue
                .MarkCompletedAsync(
                    workItem,
                    result,
                    timeProvider.GetUtcNow(),
                    cancellationToken)
                .ConfigureAwait(false);

            logger.LogInformation(
                "Crawl job {CrawlJobId} completed as {Outcome}; observed {ObservedDocuments} documents with {IssueCount} scan issues.",
                workItem.CrawlJobId,
                result.Outcome,
                result.ObservedDocuments,
                result.NonFatalIssues);

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // A cooperative host shutdown happens after LocalSourceScanService has already made its ScanRun
            // non-authoritative. Releasing the queue lease lets the same requested job be attempted again after a
            // later startup without inventing a terminal failure for normal application shutdown.
            await ReleaseAfterCancellationAsync(workItem).ConfigureAwait(false);
            throw;
        }
        catch (Exception exception)
        {
            var errorSummary = BuildErrorSummary(exception);

            // Persist failure with an independent token. If cancellation races a real I/O failure, leaving the row
            // silently Leased is less useful than recording the terminal error that already occurred.
            await workQueue
                .MarkFailedAsync(
                    workItem,
                    errorSummary,
                    timeProvider.GetUtcNow(),
                    CancellationToken.None)
                .ConfigureAwait(false);

            logger.LogError(
                exception,
                "Crawl job {CrawlJobId} failed while executing work item {WorkItemId}.",
                workItem.CrawlJobId,
                workItem.WorkItemId);

            return true;
        }
    }

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
            // Do not replace the caller's cancellation with a cleanup exception. The row remains Leased and,
            // because this milestone does not auto-reclaim leases, fails closed rather than running concurrently.
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
