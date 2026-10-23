using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Persistence.Sqlite.Jobs;
using Sasd.Crawler.Persistence.Sqlite.Sources;

namespace Sasd.Crawler.Persistence.Sqlite.Tests.Jobs;

/// <summary>
/// Verifies durable queue transitions and lease ownership against a real SQLite database.
/// </summary>
public sealed class SqliteCrawlWorkQueueTests
{
    [Fact]
    public async Task Queue_Lease_Complete_PersistsSuccessfulJobSummary()
    {
        using var fixture = new TemporaryDatabase();
        var (queue, source) = await fixture.CreateQueueAndSourceAsync();

        var jobId = await queue.EnqueueLocalFullScanAsync(
            source.Id,
            new LocalScanOptions(maximumDepth: 2),
            Utc(20, 0),
            CancellationToken.None);

        var queued = await queue.GetJobAsync(jobId, CancellationToken.None);
        Assert.NotNull(queued);
        Assert.Equal(CrawlJobStatus.Queued, queued.Status);

        var lease = await queue.TryLeaseNextAsync(
            "worker-a",
            Utc(20, 1),
            TimeSpan.FromMinutes(30),
            CancellationToken.None);

        Assert.NotNull(lease);
        Assert.Equal(jobId, lease.CrawlJobId);
        Assert.Equal(source.Id, lease.SourceId);
        Assert.Equal(2, lease.MaximumDepth);
        Assert.Equal(1, lease.Attempt);
        Assert.Equal("worker-a", lease.LeaseOwner);

        Assert.Null(await queue.TryLeaseNextAsync(
            "worker-b",
            Utc(20, 2),
            TimeSpan.FromMinutes(30),
            CancellationToken.None));

        var scanResult = new LocalSourceScanResult(
            ScanRunId.New(),
            ScanRunOutcome.Succeeded,
            ObservedDocuments: 12,
            NonFatalIssues: 0,
            NewlyMissingDocuments: 1);

        await queue.MarkCompletedAsync(
            lease,
            scanResult,
            Utc(20, 3),
            CancellationToken.None);

        var completed = await queue.GetJobAsync(jobId, CancellationToken.None);
        Assert.NotNull(completed);
        Assert.Equal(CrawlJobStatus.Succeeded, completed.Status);
        Assert.Equal(12, completed.ObservedDocuments);
        Assert.Equal(0, completed.NonFatalIssues);
        Assert.Equal(1, completed.NewlyMissingDocuments);
        Assert.Equal(Utc(20, 3), completed.CompletedAtUtc);
    }

    [Fact]
    public async Task PartialScan_CompletesWorkButKeepsDistinctPartialJobStatus()
    {
        using var fixture = new TemporaryDatabase();
        var (queue, source) = await fixture.CreateQueueAndSourceAsync();

        var jobId = await queue.EnqueueLocalFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            Utc(20, 0),
            CancellationToken.None);
        var lease = Assert.NotNull(await queue.TryLeaseNextAsync(
            "worker-a",
            Utc(20, 1),
            TimeSpan.FromMinutes(30),
            CancellationToken.None));

        await queue.MarkCompletedAsync(
            lease,
            new LocalSourceScanResult(
                ScanRunId.New(),
                ScanRunOutcome.Partial,
                ObservedDocuments: 4,
                NonFatalIssues: 2,
                NewlyMissingDocuments: 0),
            Utc(20, 2),
            CancellationToken.None);

        var snapshot = await queue.GetJobAsync(jobId, CancellationToken.None);
        Assert.NotNull(snapshot);
        Assert.Equal(CrawlJobStatus.Partial, snapshot.Status);
        Assert.Equal(2, snapshot.NonFatalIssues);
    }

    [Fact]
    public async Task ReleaseLease_ReturnsItemToPendingAndIncrementsAttemptOnNextLease()
    {
        using var fixture = new TemporaryDatabase();
        var (queue, source) = await fixture.CreateQueueAndSourceAsync();

        var jobId = await queue.EnqueueLocalFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            Utc(20, 0),
            CancellationToken.None);
        var first = Assert.NotNull(await queue.TryLeaseNextAsync(
            "worker-a",
            Utc(20, 1),
            TimeSpan.FromMinutes(30),
            CancellationToken.None));

        await queue.ReleaseLeaseAsync(first, Utc(20, 2), CancellationToken.None);

        var queued = await queue.GetJobAsync(jobId, CancellationToken.None);
        Assert.NotNull(queued);
        Assert.Equal(CrawlJobStatus.Queued, queued.Status);

        var second = Assert.NotNull(await queue.TryLeaseNextAsync(
            "worker-b",
            Utc(20, 3),
            TimeSpan.FromMinutes(30),
            CancellationToken.None));

        Assert.Equal(first.WorkItemId, second.WorkItemId);
        Assert.Equal(2, second.Attempt);
        Assert.Equal("worker-b", second.LeaseOwner);
    }

    [Fact]
    public async Task StaleOwner_CannotCompleteAnotherWorkersLease()
    {
        using var fixture = new TemporaryDatabase();
        var (queue, source) = await fixture.CreateQueueAndSourceAsync();

        await queue.EnqueueLocalFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            Utc(20, 0),
            CancellationToken.None);
        var lease = Assert.NotNull(await queue.TryLeaseNextAsync(
            "worker-a",
            Utc(20, 1),
            TimeSpan.FromMinutes(30),
            CancellationToken.None));

        var forged = lease with { LeaseOwner = "worker-b" };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => queue.MarkCompletedAsync(
                forged,
                new LocalSourceScanResult(
                    ScanRunId.New(),
                    ScanRunOutcome.Succeeded,
                    1,
                    0,
                    0),
                Utc(20, 2),
                CancellationToken.None));
    }

    [Fact]
    public async Task MarkFailed_PersistsTerminalFailureSummary()
    {
        using var fixture = new TemporaryDatabase();
        var (queue, source) = await fixture.CreateQueueAndSourceAsync();

        var jobId = await queue.EnqueueLocalFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            Utc(20, 0),
            CancellationToken.None);
        var lease = Assert.NotNull(await queue.TryLeaseNextAsync(
            "worker-a",
            Utc(20, 1),
            TimeSpan.FromMinutes(30),
            CancellationToken.None));

        await queue.MarkFailedAsync(
            lease,
            "IOException: source disappeared",
            Utc(20, 2),
            CancellationToken.None);

        var failed = await queue.GetJobAsync(jobId, CancellationToken.None);
        Assert.NotNull(failed);
        Assert.Equal(CrawlJobStatus.Failed, failed.Status);
        Assert.Contains("source disappeared", failed.LastError, StringComparison.Ordinal);
    }

    private static DateTimeOffset Utc(int hour, int minute)
        => new(2026, 10, 23, hour, minute, 0, TimeSpan.Zero);

    private sealed class TemporaryDatabase : IDisposable
    {
        private readonly string directory = Path.Combine(
            Path.GetTempPath(),
            "SasdCrawlerQueueTests",
            Guid.NewGuid().ToString("N"));

        public TemporaryDatabase()
        {
            Directory.CreateDirectory(directory);
            DatabasePath = Path.Combine(directory, "control.db");
        }

        public string DatabasePath { get; }

        public async Task<(SqliteCrawlWorkQueue Queue, SourceDefinition Source)> CreateQueueAndSourceAsync()
        {
            var controlStore = new SqliteControlStore(DatabasePath);
            var sources = new SqliteSourceRepository(controlStore);
            var queue = new SqliteCrawlWorkQueue(controlStore);

            await sources.InitializeAsync(CancellationToken.None);

            var source = SourceDefinition.CreateLocalDirectory(
                "Data",
                "file:///C:/Data/",
                Utc(19, 0));
            Assert.True(await sources.TryAddAsync(source, CancellationToken.None));

            return (queue, source);
        }

        public void Dispose()
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
