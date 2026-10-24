using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Tests.Crawling;

/// <summary>
/// Verifies full-scan orchestration and, especially, the authoritative-reconciliation boundary.
/// </summary>
public sealed class LocalSourceScanServiceTests
{
    [Fact]
    public async Task SuccessfulScan_CompletesAuthoritativelyAndReturnsReconciliationCount()
    {
        var source = CreateSource();
        var connector = new SequenceConnector(
        [
            File("file:///C:/Data/a.txt", "a.txt"),
            File("file:///C:/Data/b.txt", "b.txt"),
        ]);
        var registry = new RecordingDocumentRegistry
        {
            CompletionResult = new ScanCompletionResult(NewlyMissingDocuments: 3),
        };
        var service = CreateService(source, connector, registry);

        var result = await service.RunFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            CancellationToken.None);

        Assert.Equal(ScanRunOutcome.Succeeded, result.Outcome);
        Assert.Equal(2, result.ObservedDocuments);
        Assert.Equal(0, result.NonFatalIssues);
        Assert.Equal(3, result.NewlyMissingDocuments);
        Assert.Equal(ScanRunOutcome.Succeeded, registry.CompletedOutcome);
        Assert.Null(registry.AbortedOutcome);
        Assert.Equal(2, registry.Recorded.Count);
    }

    [Fact]
    public async Task Loc005_ItemIssue_ContinuesCrawlButSuppressesAuthoritativeReconciliation()
    {
        var source = CreateSource();
        var connector = new SequenceConnector(
        [
            File("file:///C:/Data/a.txt", "a.txt"),
            new LocalScanIssueReported(@"C:\Data\locked.txt", "UnauthorizedAccessException: denied"),
            File("file:///C:/Data/b.txt", "b.txt"),
        ]);
        var registry = new RecordingDocumentRegistry();
        var service = CreateService(source, connector, registry);

        var result = await service.RunFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            CancellationToken.None);

        Assert.Equal(ScanRunOutcome.Partial, result.Outcome);
        Assert.Equal(2, result.ObservedDocuments);
        Assert.Equal(1, result.NonFatalIssues);
        Assert.Equal(0, result.NewlyMissingDocuments);
        Assert.Equal(ScanRunOutcome.Partial, registry.CompletedOutcome);
        Assert.Equal(1, registry.CompletedIssueCount);
        Assert.Null(registry.AbortedOutcome);
    }

    [Fact]
    public async Task Reconciliation_FatalTraversalFailure_AbortsWithoutCompletingScan()
    {
        var source = CreateSource();
        var registry = new RecordingDocumentRegistry();
        var service = CreateService(
            source,
            new ThrowingConnector(new IOException("root traversal failed")),
            registry);

        await Assert.ThrowsAsync<IOException>(
            () => service.RunFullScanAsync(
                source.Id,
                new LocalScanOptions(),
                CancellationToken.None));

        Assert.Null(registry.CompletedOutcome);
        Assert.Equal(ScanRunOutcome.Failed, registry.AbortedOutcome);
    }

    [Fact]
    public async Task Reconciliation_CancelledScan_AbortsWithoutCompletingScan()
    {
        var source = CreateSource();
        var registry = new RecordingDocumentRegistry();
        using var cancellation = new CancellationTokenSource();
        var connector = new CancellingConnector(cancellation);
        var service = CreateService(source, connector, registry);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.RunFullScanAsync(
                source.Id,
                new LocalScanOptions(),
                cancellation.Token));

        Assert.Null(registry.CompletedOutcome);
        Assert.Equal(ScanRunOutcome.Cancelled, registry.AbortedOutcome);
    }

    [Fact]
    public async Task DisabledSource_IsRejectedBeforeARegistryScanStarts()
    {
        var source = CreateSource();
        source.SetEnabled(
            isEnabled: false,
            new DateTimeOffset(2026, 10, 22, 20, 0, 0, TimeSpan.Zero));
        var registry = new RecordingDocumentRegistry();
        var service = CreateService(source, new SequenceConnector([]), registry);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RunFullScanAsync(
                source.Id,
                new LocalScanOptions(),
                CancellationToken.None));

        Assert.False(registry.BeginCalled);
    }

    private static LocalSourceScanService CreateService(
        SourceDefinition source,
        ILocalFileSystemConnector connector,
        RecordingDocumentRegistry registry)
        => new(
            new SingleSourceRepository(source),
            registry,
            connector,
            new FixedTimeProvider(new DateTimeOffset(2026, 10, 22, 20, 0, 0, TimeSpan.Zero)));

    private static SourceDefinition CreateSource()
        => SourceDefinition.CreateLocalDirectory(
            "Data",
            "file:///C:/Data/",
            new DateTimeOffset(2026, 10, 22, 19, 0, 0, TimeSpan.Zero));

    private static LocalFileObserved File(string locator, string name)
        => new(
            new FileObservation(
                locator,
                name,
                fileSize: 10,
                createdAtUtc: new DateTimeOffset(2026, 10, 20, 10, 0, 0, TimeSpan.Zero),
                modifiedAtUtc: new DateTimeOffset(2026, 10, 21, 10, 0, 0, TimeSpan.Zero)));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class SingleSourceRepository(SourceDefinition source) : ISourceRepository
    {
        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<SourceDefinition>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<SourceDefinition>>([source]);

        public Task<SourceDefinition?> GetByIdAsync(SourceId sourceId, CancellationToken cancellationToken)
            => Task.FromResult(source.Id == sourceId ? source : null);

        public Task<SourceDefinition?> FindByCanonicalLocationAsync(
            string canonicalLocation,
            CancellationToken cancellationToken)
            => Task.FromResult(
                string.Equals(source.CanonicalLocation, canonicalLocation, StringComparison.OrdinalIgnoreCase)
                    ? source
                    : null);

        public Task<bool> TryAddAsync(SourceDefinition candidate, CancellationToken cancellationToken)
            => Task.FromResult(false);

        public Task UpdateAsync(SourceDefinition candidate, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class SequenceConnector(IReadOnlyList<LocalScanEvent> events) : ILocalFileSystemConnector
    {
        public IEnumerable<LocalScanEvent> Enumerate(
            SourceDefinition source,
            LocalScanOptions options,
            CancellationToken cancellationToken)
        {
            foreach (var scanEvent in events)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return scanEvent;
            }
        }
    }

    private sealed class ThrowingConnector(Exception exception) : ILocalFileSystemConnector
    {
        public IEnumerable<LocalScanEvent> Enumerate(
            SourceDefinition source,
            LocalScanOptions options,
            CancellationToken cancellationToken)
        {
            // Keeping this an iterator ensures the failure occurs during enumeration, after BeginFullScanAsync.
            if (DateTime.UtcNow.Ticks >= 0)
            {
                throw exception;
            }

            yield break;
        }
    }

    private sealed class CancellingConnector(CancellationTokenSource cancellation) : ILocalFileSystemConnector
    {
        public IEnumerable<LocalScanEvent> Enumerate(
            SourceDefinition source,
            LocalScanOptions options,
            CancellationToken cancellationToken)
        {
            cancellation.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
            yield break;
        }
    }

    private sealed class RecordingDocumentRegistry : IDocumentRegistry
    {
        private readonly ScanRunId scanRunId = ScanRunId.New();

        public bool BeginCalled { get; private set; }

        public List<FileObservation> Recorded { get; } = [];

        public ScanRunOutcome? CompletedOutcome { get; private set; }

        public int CompletedIssueCount { get; private set; }

        public ScanRunOutcome? AbortedOutcome { get; private set; }

        public ScanCompletionResult CompletionResult { get; set; } = new(0);

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<ScanRunId> BeginFullScanAsync(
            SourceId sourceId,
            DateTimeOffset startedAtUtc,
            CancellationToken cancellationToken)
        {
            BeginCalled = true;
            return Task.FromResult(scanRunId);
        }

        public Task RecordSeenAsync(
            ScanRunId candidateScanRunId,
            SourceId sourceId,
            FileObservation observation,
            DateTimeOffset observedAtUtc,
            CancellationToken cancellationToken)
        {
            Assert.Equal(scanRunId, candidateScanRunId);
            Recorded.Add(observation);
            return Task.CompletedTask;
        }

        public Task<ScanCompletionResult> CompleteFullScanAsync(
            ScanRunId candidateScanRunId,
            SourceId sourceId,
            ScanRunOutcome outcome,
            int issueCount,
            DateTimeOffset completedAtUtc,
            CancellationToken cancellationToken)
        {
            CompletedOutcome = outcome;
            CompletedIssueCount = issueCount;
            return Task.FromResult(CompletionResult);
        }

        public Task AbortFullScanAsync(
            ScanRunId candidateScanRunId,
            SourceId sourceId,
            ScanRunOutcome outcome,
            int issueCount,
            string? errorSummary,
            DateTimeOffset completedAtUtc,
            CancellationToken cancellationToken)
        {
            AbortedOutcome = outcome;
            return Task.CompletedTask;
        }

        public Task<DocumentRecord?> GetDocumentAsync(
            DocumentId documentId,
            CancellationToken cancellationToken)
            => Task.FromResult<DocumentRecord?>(null);

        public Task<IReadOnlyList<DocumentRecord>> ListDocumentsSeenInScanAsync(
            ScanRunId scanRunId,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<DocumentRecord>>([]);

        public Task<IReadOnlyList<DocumentRecord>> ListDocumentsAsync(
            SourceId sourceId,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<DocumentRecord>>([]);
    }
}
