using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Persistence.Sqlite.Crawling;
using Sasd.Crawler.Persistence.Sqlite.Sources;

namespace Sasd.Crawler.Persistence.Sqlite.Tests.Crawling;

/// <summary>
/// Verifies durable document identity, LastSeen state and fail-closed reconciliation against real SQLite.
/// </summary>
public sealed class SqliteDocumentRegistryTests
{
    [Fact]
    public async Task Id001_RepeatedObservation_PreservesDocumentIdAndUpdatesMetadata()
    {
        using var fixture = new TemporaryDatabase();
        var (sourceRepository, registry, source) = await fixture.CreateInitializedSourceAsync();

        var scan1 = await registry.BeginFullScanAsync(source.Id, Utc(20, 0), CancellationToken.None);
        await registry.RecordSeenAsync(
            scan1,
            source.Id,
            File("file:///C:/Data/a.txt", "a.txt", 10, Utc(19, 0)),
            Utc(20, 1),
            CancellationToken.None);
        await registry.CompleteFullScanAsync(
            scan1,
            source.Id,
            ScanRunOutcome.Succeeded,
            issueCount: 0,
            Utc(20, 2),
            CancellationToken.None);

        var first = Assert.Single(await registry.ListDocumentsAsync(source.Id, CancellationToken.None));

        var scan2 = await registry.BeginFullScanAsync(source.Id, Utc(21, 0), CancellationToken.None);
        await registry.RecordSeenAsync(
            scan2,
            source.Id,
            File("file:///C:/Data/a.txt", "a.txt", 25, Utc(20, 30)),
            Utc(21, 1),
            CancellationToken.None);
        await registry.CompleteFullScanAsync(
            scan2,
            source.Id,
            ScanRunOutcome.Succeeded,
            issueCount: 0,
            Utc(21, 2),
            CancellationToken.None);

        var second = Assert.Single(await registry.ListDocumentsAsync(source.Id, CancellationToken.None));

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(25, second.FileSize);
        Assert.Equal(scan2, second.LastSeenScanRunId);
        Assert.Equal(Utc(21, 1), second.LastSeenAtUtc);
        Assert.False(second.IsMissing);
        Assert.NotNull(await sourceRepository.GetByIdAsync(source.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Reconciliation_SuccessfulFullScan_MarksOnlyUnseenDocumentsMissing()
    {
        using var fixture = new TemporaryDatabase();
        var (_, registry, source) = await fixture.CreateInitializedSourceAsync();

        var scan1 = await registry.BeginFullScanAsync(source.Id, Utc(20, 0), CancellationToken.None);
        await registry.RecordSeenAsync(scan1, source.Id, File("file:///C:/Data/a.txt", "a.txt"), Utc(20, 1), CancellationToken.None);
        await registry.RecordSeenAsync(scan1, source.Id, File("file:///C:/Data/b.txt", "b.txt"), Utc(20, 1), CancellationToken.None);
        await registry.CompleteFullScanAsync(scan1, source.Id, ScanRunOutcome.Succeeded, 0, Utc(20, 2), CancellationToken.None);

        var scan2 = await registry.BeginFullScanAsync(source.Id, Utc(21, 0), CancellationToken.None);
        await registry.RecordSeenAsync(scan2, source.Id, File("file:///C:/Data/a.txt", "a.txt"), Utc(21, 1), CancellationToken.None);
        var completion = await registry.CompleteFullScanAsync(
            scan2,
            source.Id,
            ScanRunOutcome.Succeeded,
            0,
            Utc(21, 2),
            CancellationToken.None);

        var documents = await registry.ListDocumentsAsync(source.Id, CancellationToken.None);
        var a = documents.Single(document => document.DisplayName == "a.txt");
        var b = documents.Single(document => document.DisplayName == "b.txt");

        Assert.Equal(1, completion.NewlyMissingDocuments);
        Assert.False(a.IsMissing);
        Assert.True(b.IsMissing);
        Assert.Equal(Utc(21, 2), b.MissingSinceUtc!.Value);
    }

    [Fact]
    public async Task Reconciliation_PartialScan_NeverMarksUnseenDocumentsMissing()
    {
        using var fixture = new TemporaryDatabase();
        var (_, registry, source) = await fixture.CreateInitializedSourceAsync();
        await SeedTwoDocumentsAsync(registry, source);

        var partial = await registry.BeginFullScanAsync(source.Id, Utc(21, 0), CancellationToken.None);
        await registry.RecordSeenAsync(partial, source.Id, File("file:///C:/Data/a.txt", "a.txt"), Utc(21, 1), CancellationToken.None);

        var completion = await registry.CompleteFullScanAsync(
            partial,
            source.Id,
            ScanRunOutcome.Partial,
            issueCount: 1,
            Utc(21, 2),
            CancellationToken.None);

        Assert.Equal(0, completion.NewlyMissingDocuments);
        Assert.All(
            await registry.ListDocumentsAsync(source.Id, CancellationToken.None),
            document => Assert.False(document.IsMissing));
    }

    [Theory]
    [InlineData(ScanRunOutcome.Failed)]
    [InlineData(ScanRunOutcome.Cancelled)]
    public async Task Reconciliation_AbortedScan_NeverMarksUnseenDocumentsMissing(ScanRunOutcome outcome)
    {
        using var fixture = new TemporaryDatabase();
        var (_, registry, source) = await fixture.CreateInitializedSourceAsync();
        await SeedTwoDocumentsAsync(registry, source);

        var aborted = await registry.BeginFullScanAsync(source.Id, Utc(21, 0), CancellationToken.None);
        await registry.RecordSeenAsync(aborted, source.Id, File("file:///C:/Data/a.txt", "a.txt"), Utc(21, 1), CancellationToken.None);
        await registry.AbortFullScanAsync(
            aborted,
            source.Id,
            outcome,
            issueCount: 0,
            "test abort",
            Utc(21, 2),
            CancellationToken.None);

        Assert.All(
            await registry.ListDocumentsAsync(source.Id, CancellationToken.None),
            document => Assert.False(document.IsMissing));
    }

    [Fact]
    public async Task Reconciliation_ReappearingDocument_ClearsMissingMarker()
    {
        using var fixture = new TemporaryDatabase();
        var (_, registry, source) = await fixture.CreateInitializedSourceAsync();
        await SeedTwoDocumentsAsync(registry, source);

        var scan2 = await registry.BeginFullScanAsync(source.Id, Utc(21, 0), CancellationToken.None);
        await registry.RecordSeenAsync(scan2, source.Id, File("file:///C:/Data/a.txt", "a.txt"), Utc(21, 1), CancellationToken.None);
        await registry.CompleteFullScanAsync(scan2, source.Id, ScanRunOutcome.Succeeded, 0, Utc(21, 2), CancellationToken.None);

        var scan3 = await registry.BeginFullScanAsync(source.Id, Utc(22, 0), CancellationToken.None);
        await registry.RecordSeenAsync(scan3, source.Id, File("file:///C:/Data/a.txt", "a.txt"), Utc(22, 1), CancellationToken.None);
        await registry.RecordSeenAsync(scan3, source.Id, File("file:///C:/Data/b.txt", "b.txt"), Utc(22, 1), CancellationToken.None);
        await registry.CompleteFullScanAsync(scan3, source.Id, ScanRunOutcome.Succeeded, 0, Utc(22, 2), CancellationToken.None);

        Assert.All(
            await registry.ListDocumentsAsync(source.Id, CancellationToken.None),
            document => Assert.False(document.IsMissing));
    }

    private static async Task SeedTwoDocumentsAsync(
        SqliteDocumentRegistry registry,
        SourceDefinition source)
    {
        var scan = await registry.BeginFullScanAsync(source.Id, Utc(20, 0), CancellationToken.None);
        await registry.RecordSeenAsync(scan, source.Id, File("file:///C:/Data/a.txt", "a.txt"), Utc(20, 1), CancellationToken.None);
        await registry.RecordSeenAsync(scan, source.Id, File("file:///C:/Data/b.txt", "b.txt"), Utc(20, 1), CancellationToken.None);
        await registry.CompleteFullScanAsync(scan, source.Id, ScanRunOutcome.Succeeded, 0, Utc(20, 2), CancellationToken.None);
    }

    private static FileObservation File(
        string locator,
        string name,
        long size = 10,
        DateTimeOffset? modifiedAtUtc = null)
        => new(
            locator,
            name,
            size,
            createdAtUtc: Utc(18, 0),
            modifiedAtUtc: modifiedAtUtc ?? Utc(19, 0));

    private static DateTimeOffset Utc(int hour, int minute)
        => new(2026, 10, 22, hour, minute, 0, TimeSpan.Zero);

    private sealed class TemporaryDatabase : IDisposable
    {
        private readonly string directory = Path.Combine(
            Path.GetTempPath(),
            "SasdCrawlerDocumentRegistryTests",
            Guid.NewGuid().ToString("N"));

        public TemporaryDatabase()
        {
            Directory.CreateDirectory(directory);
            DatabasePath = Path.Combine(directory, "control.db");
        }

        public string DatabasePath { get; }

        public async Task<(SqliteSourceRepository SourceRepository, SqliteDocumentRegistry Registry, SourceDefinition Source)>
            CreateInitializedSourceAsync()
        {
            var controlStore = new SqliteControlStore(DatabasePath);
            var sourceRepository = new SqliteSourceRepository(controlStore);
            var registry = new SqliteDocumentRegistry(controlStore);

            await sourceRepository.InitializeAsync(CancellationToken.None);

            var source = SourceDefinition.CreateLocalDirectory(
                "Data",
                "file:///C:/Data/",
                Utc(17, 0));
            Assert.True(await sourceRepository.TryAddAsync(source, CancellationToken.None));

            return (sourceRepository, registry, source);
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
