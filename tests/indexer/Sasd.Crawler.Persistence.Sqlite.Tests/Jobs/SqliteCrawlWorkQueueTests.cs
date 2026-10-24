using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Application.Jobs;
using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Persistence.Sqlite.Crawling;
using Sasd.Crawler.Persistence.Sqlite.Jobs;
using Sasd.Crawler.Persistence.Sqlite.Sources;

namespace Sasd.Crawler.Persistence.Sqlite.Tests.Jobs;

/// <summary>
/// Verifies durable queue transitions, document follow-up work and lease ownership against real SQLite.
/// </summary>
public sealed class SqliteCrawlWorkQueueTests
{
    [Fact]
    public async Task Queue_Lease_CompleteWithoutFollowUps_PersistsSuccessfulJobSummary()
    {
        using var fixture = new TemporaryDatabase();
        var (queue, _, _, source) = await fixture.CreateAsync();

        var jobId = await queue.EnqueueLocalFullScanAsync(
            source.Id,
            new LocalScanOptions(maximumDepth: 2),
            Utc(20, 0),
            CancellationToken.None);

        var lease = await queue.TryLeaseNextAsync(
            "worker-a",
            Utc(20, 1),
            TimeSpan.FromMinutes(30),
            CancellationToken.None);
        Assert.NotNull(lease);

        await queue.CompleteFullScanAsync(
            lease!,
            ScanResult(ScanRunOutcome.Succeeded, observed: 12, issues: 0, missing: 1),
            [],
            Utc(20, 3),
            CancellationToken.None);

        var completed = await queue.GetJobAsync(jobId, CancellationToken.None);
        Assert.NotNull(completed);
        Assert.Equal(CrawlJobStatus.Succeeded, completed!.Status);
        Assert.Equal(12, completed.ObservedDocuments);
        Assert.Equal(1, completed.NewlyMissingDocuments);
        Assert.Equal(Utc(20, 3), completed.CompletedAtUtc);
    }

    [Fact]
    public async Task FullScan_WithDocumentFollowUp_RemainsRunningUntilProcessingCompletes()
    {
        using var fixture = new TemporaryDatabase();
        var (queue, registry, _, source) = await fixture.CreateAsync();
        var document = await SeedDocumentAsync(registry, source);

        var jobId = await queue.EnqueueLocalFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            Utc(21, 0),
            CancellationToken.None);
        var scanLease = await queue.TryLeaseNextAsync(
            "worker-a",
            Utc(21, 1),
            TimeSpan.FromMinutes(30),
            CancellationToken.None);
        Assert.NotNull(scanLease);

        await queue.CompleteFullScanAsync(
            scanLease!,
            ScanResult(ScanRunOutcome.Succeeded, 1, 0, 0),
            [new DocumentWorkRequest(document.Id, WorkItemStage.IndexDocument)],
            Utc(21, 2),
            CancellationToken.None);

        var running = await queue.GetJobAsync(jobId, CancellationToken.None);
        Assert.NotNull(running);
        Assert.Equal(CrawlJobStatus.Running, running!.Status);
        Assert.Null(running.CompletedAtUtc);

        var processingLease = await queue.TryLeaseNextAsync(
            "worker-a",
            Utc(21, 3),
            TimeSpan.FromMinutes(30),
            CancellationToken.None);
        Assert.NotNull(processingLease);
        Assert.Equal(WorkItemStage.IndexDocument, processingLease!.Stage);
        Assert.Equal(document.Id, processingLease.DocumentId);

        await queue.CompleteProcessingAsync(
            processingLease,
            Utc(21, 4),
            CancellationToken.None);

        var completed = await queue.GetJobAsync(jobId, CancellationToken.None);
        Assert.NotNull(completed);
        Assert.Equal(CrawlJobStatus.Succeeded, completed!.Status);
        Assert.Equal(Utc(21, 4), completed.CompletedAtUtc);
    }

    [Fact]
    public async Task PartialScan_WithFollowUp_FinalizesAsPartialAfterProcessing()
    {
        using var fixture = new TemporaryDatabase();
        var (queue, registry, _, source) = await fixture.CreateAsync();
        var document = await SeedDocumentAsync(registry, source);

        var jobId = await queue.EnqueueLocalFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            Utc(22, 0),
            CancellationToken.None);
        var scanLease = await queue.TryLeaseNextAsync(
            "worker-a",
            Utc(22, 1),
            TimeSpan.FromMinutes(30),
            CancellationToken.None);
        Assert.NotNull(scanLease);

        await queue.CompleteFullScanAsync(
            scanLease!,
            ScanResult(ScanRunOutcome.Partial, 1, 2, 0),
            [new DocumentWorkRequest(document.Id, WorkItemStage.IndexDocument)],
            Utc(22, 2),
            CancellationToken.None);

        var processing = await queue.TryLeaseNextAsync(
            "worker-a",
            Utc(22, 3),
            TimeSpan.FromMinutes(30),
            CancellationToken.None);
        Assert.NotNull(processing);

        await queue.CompleteProcessingAsync(processing!, Utc(22, 4), CancellationToken.None);

        var snapshot = await queue.GetJobAsync(jobId, CancellationToken.None);
        Assert.NotNull(snapshot);
        Assert.Equal(CrawlJobStatus.Partial, snapshot!.Status);
        Assert.Equal(2, snapshot.NonFatalIssues);
    }

    [Fact]
    public async Task ReleaseLease_ReturnsItemToPendingAndIncrementsAttemptOnNextLease()
    {
        using var fixture = new TemporaryDatabase();
        var (queue, _, _, source) = await fixture.CreateAsync();

        var jobId = await queue.EnqueueLocalFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            Utc(20, 0),
            CancellationToken.None);
        var first = await queue.TryLeaseNextAsync(
            "worker-a",
            Utc(20, 1),
            TimeSpan.FromMinutes(30),
            CancellationToken.None);
        Assert.NotNull(first);

        await queue.ReleaseLeaseAsync(first!, Utc(20, 2), CancellationToken.None);

        var queued = await queue.GetJobAsync(jobId, CancellationToken.None);
        Assert.NotNull(queued);
        Assert.Equal(CrawlJobStatus.Queued, queued!.Status);

        var second = await queue.TryLeaseNextAsync(
            "worker-b",
            Utc(20, 3),
            TimeSpan.FromMinutes(30),
            CancellationToken.None);
        Assert.NotNull(second);

        Assert.Equal(first.WorkItemId, second!.WorkItemId);
        Assert.Equal(2, second.Attempt);
        Assert.Equal("worker-b", second.LeaseOwner);
    }

    [Fact]
    public async Task StaleOwner_CannotCompleteAnotherWorkersLease()
    {
        using var fixture = new TemporaryDatabase();
        var (queue, _, _, source) = await fixture.CreateAsync();

        await queue.EnqueueLocalFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            Utc(20, 0),
            CancellationToken.None);
        var lease = await queue.TryLeaseNextAsync(
            "worker-a",
            Utc(20, 1),
            TimeSpan.FromMinutes(30),
            CancellationToken.None);
        Assert.NotNull(lease);

        var forged = lease! with { LeaseOwner = "worker-b" };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => queue.CompleteFullScanAsync(
                forged,
                ScanResult(ScanRunOutcome.Succeeded, 1, 0, 0),
                [],
                Utc(20, 2),
                CancellationToken.None));
    }

    [Fact]
    public async Task MarkFailed_PersistsTerminalFailureSummary()
    {
        using var fixture = new TemporaryDatabase();
        var (queue, _, _, source) = await fixture.CreateAsync();

        var jobId = await queue.EnqueueLocalFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            Utc(20, 0),
            CancellationToken.None);
        var lease = await queue.TryLeaseNextAsync(
            "worker-a",
            Utc(20, 1),
            TimeSpan.FromMinutes(30),
            CancellationToken.None);
        Assert.NotNull(lease);

        await queue.MarkFailedAsync(
            lease!,
            "IOException: source disappeared",
            Utc(20, 2),
            CancellationToken.None);

        var failed = await queue.GetJobAsync(jobId, CancellationToken.None);
        Assert.NotNull(failed);
        Assert.Equal(CrawlJobStatus.Failed, failed!.Status);
        Assert.Contains("source disappeared", failed.LastError!, StringComparison.Ordinal);
    }

    private static LocalSourceScanResult ScanResult(
        ScanRunOutcome outcome,
        int observed,
        int issues,
        int missing)
        => new(ScanRunId.New(), outcome, observed, issues, missing);

    private static async Task<DocumentRecord> SeedDocumentAsync(
        SqliteDocumentRegistry registry,
        SourceDefinition source)
    {
        var scanRunId = await registry.BeginFullScanAsync(
            source.Id,
            Utc(19, 10),
            CancellationToken.None);

        await registry.RecordSeenAsync(
            scanRunId,
            source.Id,
            new FileObservation(
                "file:///C:/Data/note.txt",
                "note.txt",
                10,
                createdAtUtc: Utc(19, 0),
                modifiedAtUtc: Utc(19, 5)),
            Utc(19, 11),
            CancellationToken.None);

        await registry.CompleteFullScanAsync(
            scanRunId,
            source.Id,
            ScanRunOutcome.Succeeded,
            issueCount: 0,
            Utc(19, 12),
            CancellationToken.None);

        return Assert.Single(await registry.ListDocumentsAsync(source.Id, CancellationToken.None));
    }

    private static DateTimeOffset Utc(int hour, int minute)
        => new(2026, 10, 24, hour, minute, 0, TimeSpan.Zero);

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

        public async Task<(
            SqliteCrawlWorkQueue Queue,
            SqliteDocumentRegistry Registry,
            SqliteSourceRepository Sources,
            SourceDefinition Source)> CreateAsync()
        {
            var controlStore = new SqliteControlStore(DatabasePath);
            var sources = new SqliteSourceRepository(controlStore);
            var registry = new SqliteDocumentRegistry(controlStore);
            var queue = new SqliteCrawlWorkQueue(controlStore);

            await sources.InitializeAsync(CancellationToken.None);

            var source = SourceDefinition.CreateLocalDirectory(
                "Data",
                "file:///C:/Data/",
                Utc(19, 0));
            Assert.True(await sources.TryAddAsync(source, CancellationToken.None));

            return (queue, registry, sources, source);
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
