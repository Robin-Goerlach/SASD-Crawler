using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Application.Jobs;
using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Tests.Jobs;

/// <summary>
/// Verifies that crawl requests are validated before becoming durable background work.
/// </summary>
public sealed class CrawlJobServiceTests
{
    [Fact]
    public async Task QueueLocalFullScan_EnabledLocalSource_PersistsRequest()
    {
        var source = CreateSource();
        var repository = new SingleSourceRepository(source);
        var queue = new RecordingQueue();
        var service = new CrawlJobService(repository, queue, new FixedTimeProvider(Utc(20, 0)));

        var jobId = await service.QueueLocalFullScanAsync(
            source.Id,
            new LocalScanOptions(maximumDepth: 3),
            CancellationToken.None);

        Assert.Equal(queue.JobId, jobId);
        Assert.True(repository.InitializeCalled);
        Assert.True(queue.InitializeCalled);
        Assert.Equal(source.Id, queue.QueuedSourceId);
        Assert.Equal(3, queue.QueuedOptions?.MaximumDepth);
        Assert.Equal(Utc(20, 0), queue.RequestedAtUtc);
    }

    [Fact]
    public async Task QueueLocalFullScan_DisabledSource_IsRejectedBeforeQueueInsertion()
    {
        var source = CreateSource();
        source.SetEnabled(false, Utc(19, 30));

        var queue = new RecordingQueue();
        var service = new CrawlJobService(
            new SingleSourceRepository(source),
            queue,
            new FixedTimeProvider(Utc(20, 0)));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.QueueLocalFullScanAsync(
                source.Id,
                new LocalScanOptions(),
                CancellationToken.None));

        Assert.Null(queue.QueuedSourceId);
    }

    private static SourceDefinition CreateSource()
        => SourceDefinition.CreateLocalDirectory("Data", "file:///C:/Data/", Utc(19, 0));

    private static DateTimeOffset Utc(int hour, int minute)
        => new(2026, 10, 23, hour, minute, 0, TimeSpan.Zero);

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class SingleSourceRepository(SourceDefinition source) : ISourceRepository
    {
        public bool InitializeCalled { get; private set; }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            InitializeCalled = true;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<SourceDefinition>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<SourceDefinition>>([source]);

        public Task<SourceDefinition?> GetByIdAsync(SourceId sourceId, CancellationToken cancellationToken)
            => Task.FromResult(source.Id == sourceId ? source : null);

        public Task<SourceDefinition?> FindByCanonicalLocationAsync(
            string canonicalLocation,
            CancellationToken cancellationToken)
            => Task.FromResult<SourceDefinition?>(null);

        public Task<bool> TryAddAsync(SourceDefinition candidate, CancellationToken cancellationToken)
            => Task.FromResult(false);

        public Task UpdateAsync(SourceDefinition candidate, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class RecordingQueue : ICrawlWorkQueue
    {
        public CrawlJobId JobId { get; } = CrawlJobId.New();

        public bool InitializeCalled { get; private set; }

        public SourceId? QueuedSourceId { get; private set; }

        public LocalScanOptions? QueuedOptions { get; private set; }

        public DateTimeOffset? RequestedAtUtc { get; private set; }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            InitializeCalled = true;
            return Task.CompletedTask;
        }

        public Task<CrawlJobId> EnqueueLocalFullScanAsync(
            SourceId sourceId,
            LocalScanOptions options,
            DateTimeOffset requestedAtUtc,
            CancellationToken cancellationToken)
        {
            QueuedSourceId = sourceId;
            QueuedOptions = options;
            RequestedAtUtc = requestedAtUtc;
            return Task.FromResult(JobId);
        }

        public Task<LeasedCrawlWorkItem?> TryLeaseNextAsync(
            string leaseOwner,
            DateTimeOffset nowUtc,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken)
            => Task.FromResult<LeasedCrawlWorkItem?>(null);

        public Task CompleteFullScanAsync(
            LeasedCrawlWorkItem workItem,
            LocalSourceScanResult result,
            IReadOnlyCollection<DocumentWorkRequest> followUpWork,
            DateTimeOffset completedAtUtc,
            CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task CompleteProcessingAsync(
            LeasedCrawlWorkItem workItem,
            DateTimeOffset completedAtUtc,
            CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task MarkFailedAsync(
            LeasedCrawlWorkItem workItem,
            string errorSummary,
            DateTimeOffset completedAtUtc,
            CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task ReleaseLeaseAsync(
            LeasedCrawlWorkItem workItem,
            DateTimeOffset nextAttemptUtc,
            CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<CrawlJobSnapshot?> GetJobAsync(
            CrawlJobId jobId,
            CancellationToken cancellationToken)
            => Task.FromResult<CrawlJobSnapshot?>(null);
    }
}
