using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Indexer.LocalFileSystem;
using Sasd.Crawler.Persistence.Sqlite;
using Sasd.Crawler.Persistence.Sqlite.Crawling;
using Sasd.Crawler.Persistence.Sqlite.Sources;

namespace Sasd.Crawler.LocalScan.E2E.Tests;

/// <summary>
/// Exercises the real local connector, application coordinator and SQLite registry as one vertical path.
/// </summary>
public sealed class LocalScanEndToEndTests
{
    [Fact]
    public async Task FullScan_DeleteAndRescan_MarksOnlyDeletedFileMissing()
    {
        using var fixture = new TemporaryEnvironment();
        fixture.WriteFile("keep.txt", "keep");
        fixture.WriteFile(Path.Combine("nested", "delete-me.txt"), "delete");

        var controlStore = new SqliteControlStore(fixture.DatabasePath);
        var sourceRepository = new SqliteSourceRepository(controlStore);
        var registry = new SqliteDocumentRegistry(controlStore);
        await sourceRepository.InitializeAsync(CancellationToken.None);

        var source = SourceDefinition.CreateLocalDirectory(
            "E2E",
            new Uri(fixture.RootPath + Path.DirectorySeparatorChar, UriKind.Absolute).AbsoluteUri,
            fixture.Clock.GetUtcNow());
        Assert.True(await sourceRepository.TryAddAsync(source, CancellationToken.None));

        var service = new LocalSourceScanService(
            sourceRepository,
            registry,
            new LocalFileSystemConnector(),
            fixture.Clock);

        var first = await service.RunFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            CancellationToken.None);

        Assert.Equal(ScanRunOutcome.Succeeded, first.Outcome);
        Assert.Equal(2, first.ObservedDocuments);
        Assert.Equal(0, first.NewlyMissingDocuments);

        File.Delete(Path.Combine(fixture.RootPath, "nested", "delete-me.txt"));
        fixture.Clock.Advance(TimeSpan.FromMinutes(5));

        var second = await service.RunFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            CancellationToken.None);

        var documents = await registry.ListDocumentsAsync(source.Id, CancellationToken.None);
        var kept = documents.Single(document => document.DisplayName == "keep.txt");
        var deleted = documents.Single(document => document.DisplayName == "delete-me.txt");

        Assert.Equal(ScanRunOutcome.Succeeded, second.Outcome);
        Assert.Equal(1, second.ObservedDocuments);
        Assert.Equal(1, second.NewlyMissingDocuments);
        Assert.False(kept.IsMissing);
        Assert.True(deleted.IsMissing);
    }

    private sealed class TemporaryEnvironment : IDisposable
    {
        private readonly string parentDirectory = Path.Combine(
            Path.GetTempPath(),
            "SasdCrawlerE2E",
            Guid.NewGuid().ToString("N"));

        public TemporaryEnvironment()
        {
            RootPath = Path.Combine(parentDirectory, "source");
            Directory.CreateDirectory(RootPath);
            DatabasePath = Path.Combine(parentDirectory, "control.db");
            Clock = new AdvancingTimeProvider(
                new DateTimeOffset(2026, 10, 22, 20, 0, 0, TimeSpan.Zero));
        }

        public string RootPath { get; }

        public string DatabasePath { get; }

        public AdvancingTimeProvider Clock { get; }

        public void WriteFile(string relativePath, string content)
        {
            var path = Path.Combine(RootPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }

        public void Dispose()
        {
            if (Directory.Exists(parentDirectory))
            {
                Directory.Delete(parentDirectory, recursive: true);
            }
        }
    }

    private sealed class AdvancingTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset current = utcNow;

        public override DateTimeOffset GetUtcNow() => current;

        public void Advance(TimeSpan amount) => current = current.Add(amount);
    }
}
