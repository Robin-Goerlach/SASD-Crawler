using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Jobs;

/// <summary>
/// Validates crawl requests and stores them as durable background work.
/// </summary>
public sealed class CrawlJobService : ICrawlJobService
{
    private readonly ISourceRepository sourceRepository;
    private readonly ICrawlWorkQueue workQueue;
    private readonly TimeProvider timeProvider;

    /// <summary>Creates the crawl-job application service.</summary>
    public CrawlJobService(
        ISourceRepository sourceRepository,
        ICrawlWorkQueue workQueue,
        TimeProvider timeProvider)
    {
        this.sourceRepository = sourceRepository ?? throw new ArgumentNullException(nameof(sourceRepository));
        this.workQueue = workQueue ?? throw new ArgumentNullException(nameof(workQueue));
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        // Both ports may share the same physical SQLite control store in the desktop host, but the application
        // service intentionally depends only on their contracts.
        await sourceRepository.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await workQueue.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CrawlJobId> QueueLocalFullScanAsync(
        SourceId sourceId,
        LocalScanOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        var source = await sourceRepository.GetByIdAsync(sourceId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Source '{sourceId}' does not exist.");

        if (!source.IsEnabled)
        {
            throw new InvalidOperationException($"Source '{sourceId}' is disabled and cannot be queued.");
        }

        if (source.Kind != SourceKind.LocalDirectory)
        {
            throw new InvalidOperationException(
                $"Source '{sourceId}' is '{source.Kind}', not a local-directory source.");
        }

        return await workQueue
            .EnqueueLocalFullScanAsync(sourceId, options, timeProvider.GetUtcNow(), cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<CrawlJobSnapshot?> GetJobAsync(
        CrawlJobId jobId,
        CancellationToken cancellationToken)
        => workQueue.GetJobAsync(jobId, cancellationToken);
}
