using Microsoft.Extensions.Logging.Abstractions;
using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Application.Jobs;
using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Indexer.Workers;

namespace Sasd.Crawler.Indexer.Workers.Tests;

/// <summary>
/// Verifies one-item worker execution without running the timed BackgroundService loop.
/// </summary>
public sealed class CrawlWorkItemProcessorTests
{
    [Fact]
    public async Task NoReadyItem_ReturnsFalseWithoutInvokingScanner()
    {
        var queue = new RecordingQueue();
        var scanner = new RecordingScanner();
        var processor = CreateProcessor(queue, scanner);

        var processed = await processor.ProcessNextAsync(
            "worker",
            TimeSpan.FromMinutes(30),
            CancellationToken.None);

        Assert.False(processed);
        Assert.Equal(0, scanner.CallCount);
    }

    [Fact]
    public async Task SuccessfulScan_CompletesOwnedWorkItem()
    {
        var queue = new RecordingQueue { Lease = CreateLease() };
        var scanner = new RecordingScanner
        {
            Result = new LocalSourceScanResult(
                ScanRunId.New(),
                ScanRunOutcome.Succeeded,
                5,
                0,
                1),
        };
        var processor = CreateProcessor(queue, scanner);

        Assert.True(await processor.ProcessNextAsync(
            "worker",
            TimeSpan.FromMinutes(30),
            CancellationToken.None));

        Assert.NotNull(queue.CompletedResult);
        Assert.Null(queue.Failure);
        Assert.Equal(5, queue.CompletedResult.ObservedDocuments);
    }

    [Fact]
    public async Task ScannerFailure_MarksOwnedWorkItemFailedAndDoesNotRethrow()
    {
        var queue = new RecordingQueue { Lease = CreateLease() };
        var scanner = new RecordingScanner { Exception = new IOException("disk vanished") };
        var processor = CreateProcessor(queue, scanner);

        Assert.True(await processor.ProcessNextAsync(
            "worker",
            TimeSpan.FromMinutes(30),
            CancellationToken.None));

        Assert.Null(queue.CompletedResult);
        Assert.NotNull(queue.Failure);
        Assert.Contains("disk vanished", queue.Failure!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HostCancellation_ReleasesLeaseAndPropagatesCancellation()
    {
        var queue = new RecordingQueue { Lease = CreateLease() };
        using var cancellation = new CancellationTokenSource();
        var scanner = new RecordingScanner
        {
            BeforeRun = cancellation.Cancel,
        };
        var processor = CreateProcessor(queue, scanner);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => processor.ProcessNextAsync(
                "worker",
                TimeSpan.FromMinutes(30),
                cancellation.Token));

        Assert.True(queue.Released);
        Assert.Null(queue.Failure);
    }

    private static CrawlWorkItemProcessor CreateProcessor(
        RecordingQueue queue,
        RecordingScanner scanner)
        => new(
            queue,
            scanner,
            new FixedTimeProvider(new DateTimeOffset(2026, 10, 23, 20, 0, 0, TimeSpan.Zero)),
            NullLogger<CrawlWorkItemProcessor>.Instance);

    private static LeasedCrawlWorkItem CreateLease()
        => new(
            WorkItemId.New(),
            CrawlJobId.New(),
            SourceId.New(),
            WorkItemStage.LocalFullScan,
            MaximumDepth: null,
            Attempt: 1,
            LeaseOwner: "worker",
            LeaseUntilUtc: new DateTimeOffset(2026, 10, 23, 20, 30, 0, TimeSpan.Zero));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class RecordingScanner : ILocalSourceScanner
    {
        public int CallCount { get; private set; }

        public Action? BeforeRun { get; init; }

        public Exception? Exception { get; init; }

        public LocalSourceScanResult Result { get; init; } = new(
            ScanRunId.New(),
            ScanRunOutcome.Succeeded,
            0,
            0,
            0);

        public Task<LocalSourceScanResult> RunFullScanAsync(
            SourceId sourceId,
            LocalScanOptions options,
            CancellationToken cancellationToken)
        {
            CallCount++;
            BeforeRun?.Invoke();
            cancellationToken.ThrowIfCancellationRequested();

            if (Exception is not null)
            {
                throw Exception;
            }

            return Task.FromResult(Result);
        }
    }

    private sealed class RecordingQueue : ICrawlWorkQueue
    {
        public LeasedCrawlWorkItem? Lease { get; init; }

        public LocalSourceScanResult? CompletedResult { get; private set; }

        public string? Failure { get; private set; }

        public bool Released { get; private set; }

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<CrawlJobId> EnqueueLocalFullScanAsync(
            SourceId sourceId,
            LocalScanOptions options,
            DateTimeOffset requestedAtUtc,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<LeasedCrawlWorkItem?> TryLeaseNextAsync(
            string leaseOwner,
            DateTimeOffset nowUtc,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken)
            => Task.FromResult(Lease);

        public Task MarkCompletedAsync(
            LeasedCrawlWorkItem workItem,
            LocalSourceScanResult result,
            DateTimeOffset completedAtUtc,
            CancellationToken cancellationToken)
        {
            CompletedResult = result;
            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(
            LeasedCrawlWorkItem workItem,
            string errorSummary,
            DateTimeOffset completedAtUtc,
            CancellationToken cancellationToken)
        {
            Failure = errorSummary;
            return Task.CompletedTask;
        }

        public Task ReleaseLeaseAsync(
            LeasedCrawlWorkItem workItem,
            DateTimeOffset nextAttemptUtc,
            CancellationToken cancellationToken)
        {
            Released = true;
            return Task.CompletedTask;
        }

        public Task<CrawlJobSnapshot?> GetJobAsync(
            CrawlJobId jobId,
            CancellationToken cancellationToken)
            => Task.FromResult<CrawlJobSnapshot?>(null);
    }
}
