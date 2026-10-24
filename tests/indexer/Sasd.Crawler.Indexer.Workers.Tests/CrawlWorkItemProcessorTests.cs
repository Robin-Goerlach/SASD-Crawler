using Microsoft.Extensions.Logging.Abstractions;
using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Application.Jobs;
using Sasd.Crawler.Application.Search;
using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Indexer.Workers;

namespace Sasd.Crawler.Indexer.Workers.Tests;

/// <summary>
/// Verifies one-item discovery/index/delete execution without running the timed BackgroundService loop.
/// </summary>
public sealed class CrawlWorkItemProcessorTests
{
    [Fact]
    public async Task NoReadyItem_ReturnsFalseWithoutInvokingScanner()
    {
        var queue = new RecordingQueue();
        var scanner = new RecordingScanner();
        var processor = CreateProcessor(queue, scanner, new RecordingRegistry(), new RecordingIndexingService());

        var processed = await processor.ProcessNextAsync(
            "worker",
            TimeSpan.FromMinutes(30),
            CancellationToken.None);

        Assert.False(processed);
        Assert.Equal(0, scanner.CallCount);
    }

    [Fact]
    public async Task SuccessfulScan_QueuesSupportedDocumentsAndAuthoritativeDeletes()
    {
        var scanRunId = ScanRunId.New();
        var supported = Document("file:///C:/Data/note.txt", scanRunId);
        var unsupported = Document("file:///C:/Data/image.jpg", scanRunId);
        var missingId = DocumentId.New();

        var result = new LocalSourceScanResult(
            scanRunId,
            ScanRunOutcome.Succeeded,
            ObservedDocuments: 2,
            NonFatalIssues: 0,
            NewlyMissingDocuments: 1)
        {
            NewlyMissingDocumentIds = [missingId],
        };

        var queue = new RecordingQueue { Lease = ScanLease() };
        var registry = new RecordingRegistry { SeenDocuments = [supported, unsupported] };
        var indexing = new RecordingIndexingService
        {
            CanIndexPredicate = document => document.CanonicalLocator.EndsWith(".txt", StringComparison.OrdinalIgnoreCase),
        };
        var processor = CreateProcessor(
            queue,
            new RecordingScanner { Result = result },
            registry,
            indexing);

        Assert.True(await processor.ProcessNextAsync(
            "worker",
            TimeSpan.FromMinutes(30),
            CancellationToken.None));

        Assert.NotNull(queue.CompletedScanResult);
        Assert.Contains(
            queue.FollowUps,
            item => item.Stage == WorkItemStage.IndexDocument && item.DocumentId == supported.Id);
        Assert.DoesNotContain(
            queue.FollowUps,
            item => item.Stage == WorkItemStage.IndexDocument && item.DocumentId == unsupported.Id);
        Assert.Contains(
            queue.FollowUps,
            item => item.Stage == WorkItemStage.DeleteFromSearchIndex && item.DocumentId == missingId);
    }

    [Fact]
    public async Task IndexDocument_CurrentSupportedDocument_IsIndexedAndCompleted()
    {
        var scanRunId = ScanRunId.New();
        var document = Document("file:///C:/Data/note.txt", scanRunId);
        var queue = new RecordingQueue { Lease = DocumentLease(WorkItemStage.IndexDocument, document.Id) };
        var registry = new RecordingRegistry { DocumentById = document };
        var indexing = new RecordingIndexingService { CanIndexPredicate = _ => true };
        var processor = CreateProcessor(queue, new RecordingScanner(), registry, indexing);

        Assert.True(await processor.ProcessNextAsync(
            "worker",
            TimeSpan.FromMinutes(30),
            CancellationToken.None));

        Assert.Equal(document.Id, Assert.Single(indexing.Indexed));
        Assert.Empty(indexing.Deleted);
        Assert.True(queue.ProcessingCompleted);
    }

    [Fact]
    public async Task IndexDocument_DocumentBecameMissing_RemovesStaleIndexEntry()
    {
        var scanRunId = ScanRunId.New();
        var document = Document(
            "file:///C:/Data/note.txt",
            scanRunId,
            missingSinceUtc: new DateTimeOffset(2026, 10, 24, 10, 0, 0, TimeSpan.Zero));

        var queue = new RecordingQueue { Lease = DocumentLease(WorkItemStage.IndexDocument, document.Id) };
        var registry = new RecordingRegistry { DocumentById = document };
        var indexing = new RecordingIndexingService { CanIndexPredicate = _ => true };
        var processor = CreateProcessor(queue, new RecordingScanner(), registry, indexing);

        await processor.ProcessNextAsync("worker", TimeSpan.FromMinutes(30), CancellationToken.None);

        Assert.Empty(indexing.Indexed);
        Assert.Equal(document.Id, Assert.Single(indexing.Deleted));
        Assert.True(queue.ProcessingCompleted);
    }

    [Fact]
    public async Task DeleteStage_DeletesRequestedDocumentAndCompletesProcessing()
    {
        var documentId = DocumentId.New();
        var queue = new RecordingQueue
        {
            Lease = DocumentLease(WorkItemStage.DeleteFromSearchIndex, documentId),
        };
        var indexing = new RecordingIndexingService();
        var processor = CreateProcessor(
            queue,
            new RecordingScanner(),
            new RecordingRegistry(),
            indexing);

        await processor.ProcessNextAsync("worker", TimeSpan.FromMinutes(30), CancellationToken.None);

        Assert.Equal(documentId, Assert.Single(indexing.Deleted));
        Assert.True(queue.ProcessingCompleted);
    }

    [Fact]
    public async Task ScannerFailure_MarksOwnedWorkItemFailedAndDoesNotRethrow()
    {
        var queue = new RecordingQueue { Lease = ScanLease() };
        var scanner = new RecordingScanner { Exception = new IOException("disk vanished") };
        var processor = CreateProcessor(queue, scanner, new RecordingRegistry(), new RecordingIndexingService());

        Assert.True(await processor.ProcessNextAsync(
            "worker",
            TimeSpan.FromMinutes(30),
            CancellationToken.None));

        Assert.Null(queue.CompletedScanResult);
        Assert.NotNull(queue.Failure);
        Assert.Contains("disk vanished", queue.Failure!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HostCancellation_ReleasesLeaseAndPropagatesCancellation()
    {
        var queue = new RecordingQueue { Lease = ScanLease() };
        using var cancellation = new CancellationTokenSource();
        var scanner = new RecordingScanner { BeforeRun = cancellation.Cancel };
        var processor = CreateProcessor(queue, scanner, new RecordingRegistry(), new RecordingIndexingService());

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
        RecordingScanner scanner,
        RecordingRegistry registry,
        RecordingIndexingService indexing)
        => new(
            queue,
            scanner,
            registry,
            indexing,
            new FixedTimeProvider(new DateTimeOffset(2026, 10, 24, 11, 0, 0, TimeSpan.Zero)),
            NullLogger<CrawlWorkItemProcessor>.Instance);

    private static LeasedCrawlWorkItem ScanLease()
        => new(
            WorkItemId.New(),
            CrawlJobId.New(),
            SourceId.New(),
            WorkItemStage.LocalFullScan,
            DocumentId: null,
            MaximumDepth: null,
            Attempt: 1,
            LeaseOwner: "worker",
            LeaseUntilUtc: new DateTimeOffset(2026, 10, 24, 11, 30, 0, TimeSpan.Zero));

    private static LeasedCrawlWorkItem DocumentLease(WorkItemStage stage, DocumentId documentId)
        => new(
            WorkItemId.New(),
            CrawlJobId.New(),
            SourceId.New(),
            stage,
            documentId,
            MaximumDepth: null,
            Attempt: 1,
            LeaseOwner: "worker",
            LeaseUntilUtc: new DateTimeOffset(2026, 10, 24, 11, 30, 0, TimeSpan.Zero));

    private static DocumentRecord Document(
        string locator,
        ScanRunId scanRunId,
        DateTimeOffset? missingSinceUtc = null)
        => DocumentRecord.Restore(
            DocumentId.New(),
            SourceId.New(),
            locator,
            Path.GetFileName(new Uri(locator).LocalPath),
            fileSize: 12,
            createdAtUtc: null,
            modifiedAtUtc: new DateTimeOffset(2026, 10, 24, 10, 0, 0, TimeSpan.Zero),
            lastSeenAtUtc: new DateTimeOffset(2026, 10, 24, 10, 1, 0, TimeSpan.Zero),
            scanRunId,
            missingSinceUtc);

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

    private sealed class RecordingRegistry : IDocumentRegistry
    {
        public IReadOnlyList<DocumentRecord> SeenDocuments { get; init; } = [];

        public DocumentRecord? DocumentById { get; init; }

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<ScanRunId> BeginFullScanAsync(
            SourceId sourceId,
            DateTimeOffset startedAtUtc,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task RecordSeenAsync(
            ScanRunId scanRunId,
            SourceId sourceId,
            FileObservation observation,
            DateTimeOffset observedAtUtc,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ScanCompletionResult> CompleteFullScanAsync(
            ScanRunId scanRunId,
            SourceId sourceId,
            ScanRunOutcome outcome,
            int issueCount,
            DateTimeOffset completedAtUtc,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task AbortFullScanAsync(
            ScanRunId scanRunId,
            SourceId sourceId,
            ScanRunOutcome outcome,
            int issueCount,
            string? errorSummary,
            DateTimeOffset completedAtUtc,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<DocumentRecord?> GetDocumentAsync(
            DocumentId documentId,
            CancellationToken cancellationToken)
            => Task.FromResult(
                DocumentById?.Id == documentId
                    ? DocumentById
                    : null);

        public Task<IReadOnlyList<DocumentRecord>> ListDocumentsSeenInScanAsync(
            ScanRunId scanRunId,
            CancellationToken cancellationToken)
            => Task.FromResult(SeenDocuments);

        public Task<IReadOnlyList<DocumentRecord>> ListDocumentsAsync(
            SourceId sourceId,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<DocumentRecord>>([]);
    }

    private sealed class RecordingIndexingService : IDocumentIndexingService
    {
        public Func<DocumentRecord, bool> CanIndexPredicate { get; init; } = _ => false;

        public List<DocumentId> Indexed { get; } = [];

        public List<DocumentId> Deleted { get; } = [];

        public bool CanIndex(DocumentRecord document) => CanIndexPredicate(document);

        public Task IndexAsync(DocumentRecord document, CancellationToken cancellationToken)
        {
            Indexed.Add(document.Id);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(DocumentId documentId, CancellationToken cancellationToken)
        {
            Deleted.Add(documentId);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingQueue : ICrawlWorkQueue
    {
        public LeasedCrawlWorkItem? Lease { get; init; }

        public LocalSourceScanResult? CompletedScanResult { get; private set; }

        public IReadOnlyCollection<DocumentWorkRequest> FollowUps { get; private set; } = [];

        public bool ProcessingCompleted { get; private set; }

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

        public Task CompleteFullScanAsync(
            LeasedCrawlWorkItem workItem,
            LocalSourceScanResult result,
            IReadOnlyCollection<DocumentWorkRequest> followUpWork,
            DateTimeOffset completedAtUtc,
            CancellationToken cancellationToken)
        {
            CompletedScanResult = result;
            FollowUps = followUpWork.ToArray();
            return Task.CompletedTask;
        }

        public Task CompleteProcessingAsync(
            LeasedCrawlWorkItem workItem,
            DateTimeOffset completedAtUtc,
            CancellationToken cancellationToken)
        {
            ProcessingCompleted = true;
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
