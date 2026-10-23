using Microsoft.Extensions.Logging.Abstractions;
using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Application.Jobs;
using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Indexer.LocalFileSystem;
using Sasd.Crawler.Indexer.Workers;
using Sasd.Crawler.Persistence.Sqlite;
using Sasd.Crawler.Persistence.Sqlite.Crawling;
using Sasd.Crawler.Persistence.Sqlite.Jobs;
using Sasd.Crawler.Persistence.Sqlite.Sources;

namespace Sasd.Crawler.LocalScan.E2E.Tests;

/// <summary>
/// Verifies the real durable job -> BackgroundService -> local scan -> SQLite registry path.
/// </summary>
public sealed class QueuedBackgroundScanEndToEndTests
{
    [Fact]
    public async Task QueuedJob_BackgroundWorker_CompletesWithoutUiThreadCrawling()
    {
        using var fixture = new TemporaryEnvironment();
        fixture.WriteFile("one.txt", "one");
        fixture.WriteFile(Path.Combine("nested", "two.txt"), "two");

        var controlStore = new SqliteControlStore(fixture.DatabasePath);
        var sources = new SqliteSourceRepository(controlStore);
        var documents = new SqliteDocumentRegistry(controlStore);
        var queue = new SqliteCrawlWorkQueue(controlStore);

        await sources.InitializeAsync(CancellationToken.None);

        var source = SourceDefinition.CreateLocalDirectory(
            "Background E2E",
            new Uri(fixture.RootPath + Path.DirectorySeparatorChar, UriKind.Absolute).AbsoluteUri,
            DateTimeOffset.UtcNow);
        Assert.True(await sources.TryAddAsync(source, CancellationToken.None));

        var jobs = new CrawlJobService(sources, queue, TimeProvider.System);
        var scanner = new LocalSourceScanService(
            sources,
            documents,
            new LocalFileSystemConnector(),
            TimeProvider.System);
        var processor = new CrawlWorkItemProcessor(
            queue,
            scanner,
            TimeProvider.System,
            NullLogger<CrawlWorkItemProcessor>.Instance);
        var worker = new CrawlWorkBackgroundService(
            queue,
            processor,
            TimeProvider.System,
            new CrawlWorkerOptions
            {
                PollInterval = TimeSpan.FromMilliseconds(10),
                LeaseDuration = TimeSpan.FromMinutes(5),
            },
            NullLogger<CrawlWorkBackgroundService>.Instance);

        var jobId = await jobs.QueueLocalFullScanAsync(
            source.Id,
            new LocalScanOptions(),
            CancellationToken.None);

        await worker.StartAsync(CancellationToken.None);
        try
        {
            var snapshot = await WaitForTerminalJobAsync(queue, jobId, TimeSpan.FromSeconds(10));

            Assert.Equal(CrawlJobStatus.Succeeded, snapshot.Status);
            Assert.Equal(2, snapshot.ObservedDocuments);
            Assert.Equal(0, snapshot.NonFatalIssues);

            var registered = await documents.ListDocumentsAsync(source.Id, CancellationToken.None);
            Assert.Equal(2, registered.Count);
            Assert.All(registered, document => Assert.False(document.IsMissing));
        }
        finally
        {
            using var stopTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await worker.StopAsync(stopTimeout.Token);
            worker.Dispose();
        }
    }

    private static async Task<CrawlJobSnapshot> WaitForTerminalJobAsync(
        ICrawlWorkQueue queue,
        CrawlJobId jobId,
        TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);

        while (DateTimeOffset.UtcNow < deadline)
        {
            var snapshot = await queue.GetJobAsync(jobId, CancellationToken.None);
            if (snapshot is not null
                && snapshot.Status is CrawlJobStatus.Succeeded
                    or CrawlJobStatus.Partial
                    or CrawlJobStatus.Failed
                    or CrawlJobStatus.Cancelled)
            {
                return snapshot;
            }

            await Task.Delay(20);
        }

        throw new TimeoutException($"Crawl job '{jobId}' did not reach a terminal state within {timeout}.");
    }

    private sealed class TemporaryEnvironment : IDisposable
    {
        private readonly string parentDirectory = Path.Combine(
            Path.GetTempPath(),
            "SasdCrawlerBackgroundE2E",
            Guid.NewGuid().ToString("N"));

        public TemporaryEnvironment()
        {
            RootPath = Path.Combine(parentDirectory, "source");
            Directory.CreateDirectory(RootPath);
            DatabasePath = Path.Combine(parentDirectory, "control.db");
        }

        public string RootPath { get; }

        public string DatabasePath { get; }

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
}
