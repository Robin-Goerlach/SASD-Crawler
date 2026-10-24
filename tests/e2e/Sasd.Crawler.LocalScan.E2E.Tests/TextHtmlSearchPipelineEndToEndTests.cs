using Microsoft.Extensions.Logging.Abstractions;
using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Application.Extraction;
using Sasd.Crawler.Application.Jobs;
using Sasd.Crawler.Application.Search;
using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Extraction.Native;
using Sasd.Crawler.Indexer.LocalFileSystem;
using Sasd.Crawler.Indexer.Workers;
using Sasd.Crawler.Persistence.Sqlite;
using Sasd.Crawler.Persistence.Sqlite.Crawling;
using Sasd.Crawler.Persistence.Sqlite.Jobs;
using Sasd.Crawler.Persistence.Sqlite.Sources;
using Sasd.Crawler.Search.Lucene;

namespace Sasd.Crawler.LocalScan.E2E.Tests;

/// <summary>
/// Exercises the complete 0.1 content path from local files through durable work to Lucene.
/// </summary>
public sealed class TextHtmlSearchPipelineEndToEndTests
{
    [Fact]
    public async Task Crawl_Index_Update_AndDelete_KeepSearchResultsCurrent()
    {
        using var fixture = new TemporaryEnvironment();
        var textPath = fixture.WriteFile("note.txt", "olduniquetoken alpha");
        var htmlPath = fixture.WriteFile(
            "page.html",
            "<html lang=\"de\"><head><title>HTML Seite</title></head><body>htmluniquetoken bravo</body></html>");

        var controlStore = new SqliteControlStore(fixture.DatabasePath);
        var sources = new SqliteSourceRepository(controlStore);
        var documents = new SqliteDocumentRegistry(controlStore);
        var queue = new SqliteCrawlWorkQueue(controlStore);

        await sources.InitializeAsync(CancellationToken.None);

        var source = SourceDefinition.CreateLocalDirectory(
            "Search E2E",
            new Uri(fixture.RootPath + Path.DirectorySeparatorChar, UriKind.Absolute).AbsoluteUri,
            DateTimeOffset.UtcNow);
        Assert.True(await sources.TryAddAsync(source, CancellationToken.None));

        await using var searchIndex = new LuceneSearchIndex(
            fixture.SearchIndexPath,
            SearchAnalyzerConfiguration.CreateDefault());

        IContentExtractor extractor = new NativeTextHtmlContentExtractor(
            new NativeContentExtractionOptions());
        var indexing = new DocumentIndexingService([extractor], searchIndex);

        var jobs = new CrawlJobService(sources, queue, TimeProvider.System);
        var scanner = new LocalSourceScanService(
            sources,
            documents,
            new LocalFileSystemConnector(),
            TimeProvider.System);
        var processor = new CrawlWorkItemProcessor(
            queue,
            scanner,
            documents,
            indexing,
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

        await worker.StartAsync(CancellationToken.None);
        try
        {
            var firstJob = await jobs.QueueLocalFullScanAsync(
                source.Id,
                new LocalScanOptions(),
                CancellationToken.None);
            await WaitForSuccessAsync(queue, firstJob);

            Assert.Single((await searchIndex.SearchAsync(
                new SearchRequest("olduniquetoken"),
                CancellationToken.None)).Hits);
            Assert.Single((await searchIndex.SearchAsync(
                new SearchRequest("htmluniquetoken"),
                CancellationToken.None)).Hits);

            // Rewriting the same path preserves DocumentId in SQLite, while Lucene UpdateDocument replaces the old
            // analyzed content for that stable identity.
            File.WriteAllText(textPath, "newuniquetoken alpha");
            var secondJob = await jobs.QueueLocalFullScanAsync(
                source.Id,
                new LocalScanOptions(),
                CancellationToken.None);
            await WaitForSuccessAsync(queue, secondJob);

            Assert.Empty((await searchIndex.SearchAsync(
                new SearchRequest("olduniquetoken"),
                CancellationToken.None)).Hits);
            Assert.Single((await searchIndex.SearchAsync(
                new SearchRequest("newuniquetoken"),
                CancellationToken.None)).Hits);

            // Deletion is authorized only by the successful third full scan. The resulting DocumentId is then
            // queued as DeleteFromSearchIndex; the worker never infers deletion directly from a filesystem error.
            File.Delete(htmlPath);
            var thirdJob = await jobs.QueueLocalFullScanAsync(
                source.Id,
                new LocalScanOptions(),
                CancellationToken.None);
            await WaitForSuccessAsync(queue, thirdJob);

            Assert.Empty((await searchIndex.SearchAsync(
                new SearchRequest("htmluniquetoken"),
                CancellationToken.None)).Hits);
            Assert.Single((await searchIndex.SearchAsync(
                new SearchRequest("newuniquetoken"),
                CancellationToken.None)).Hits);

            var registryDocuments = await documents.ListDocumentsAsync(source.Id, CancellationToken.None);
            Assert.Equal(2, registryDocuments.Count);
            Assert.True(registryDocuments.Single(document => document.DisplayName == "page.html").IsMissing);
            Assert.False(registryDocuments.Single(document => document.DisplayName == "note.txt").IsMissing);
        }
        finally
        {
            using var stopTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await worker.StopAsync(stopTimeout.Token);
            worker.Dispose();
        }
    }

    private static async Task<CrawlJobSnapshot> WaitForSuccessAsync(
        ICrawlWorkQueue queue,
        CrawlJobId jobId)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(15);

        while (DateTimeOffset.UtcNow < deadline)
        {
            var snapshot = await queue.GetJobAsync(jobId, CancellationToken.None);
            if (snapshot?.Status == CrawlJobStatus.Succeeded)
            {
                return snapshot;
            }

            if (snapshot?.Status is CrawlJobStatus.Failed or CrawlJobStatus.Partial or CrawlJobStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    $"Crawl job '{jobId}' terminated unexpectedly as '{snapshot.Status}': {snapshot.LastError}");
            }

            await Task.Delay(20);
        }

        throw new TimeoutException($"Crawl job '{jobId}' did not succeed within the test timeout.");
    }

    private sealed class TemporaryEnvironment : IDisposable
    {
        private readonly string parentDirectory = Path.Combine(
            Path.GetTempPath(),
            "SasdCrawlerSearchPipelineE2E",
            Guid.NewGuid().ToString("N"));

        public TemporaryEnvironment()
        {
            RootPath = Path.Combine(parentDirectory, "source");
            Directory.CreateDirectory(RootPath);
            DatabasePath = Path.Combine(parentDirectory, "control.db");
            SearchIndexPath = Path.Combine(parentDirectory, "search-index");
        }

        public string RootPath { get; }

        public string DatabasePath { get; }

        public string SearchIndexPath { get; }

        public string WriteFile(string relativePath, string content)
        {
            var path = Path.Combine(RootPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
            return path;
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
