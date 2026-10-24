using Sasd.Crawler.Application.Extraction;
using Sasd.Crawler.Application.Search;
using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Tests.Search;

/// <summary>
/// Verifies the backend-independent extraction-to-index application service.
/// </summary>
public sealed class DocumentIndexingServiceTests
{
    [Fact]
    public async Task IndexAsync_SelectsSupportingExtractorAndCommitsMutation()
    {
        var document = CreateDocument("file:///C:/Data/example.txt");
        var unsupported = new StubExtractor(canExtract: false, text: "wrong");
        var supported = new StubExtractor(canExtract: true, text: "searchable body");
        var index = new RecordingSearchIndex();
        var service = new DocumentIndexingService([unsupported, supported], index);

        await service.IndexAsync(document, CancellationToken.None);

        Assert.Equal(0, unsupported.ExtractCalls);
        Assert.Equal(1, supported.ExtractCalls);
        Assert.NotNull(index.Upserted);
        Assert.Equal(document.Id, index.Upserted!.DocumentId);
        Assert.Equal("searchable body", index.Upserted.Content);
        Assert.Equal(document.DisplayName, index.Upserted.Title);
        Assert.Equal(1, index.CommitCalls);
    }

    [Fact]
    public async Task IndexAsync_UsesExtractorTitleAndLanguage()
    {
        var document = CreateDocument("file:///C:/Data/page.html");
        var extractor = new StubExtractor(
            canExtract: true,
            text: "Hallo Welt",
            title: "Seitentitel",
            language: "de-DE");
        var index = new RecordingSearchIndex();
        var service = new DocumentIndexingService([extractor], index);

        await service.IndexAsync(document, CancellationToken.None);

        Assert.Equal("Seitentitel", index.Upserted?.Title);
        Assert.Equal("de-de", index.Upserted?.Language);
    }

    [Fact]
    public async Task DeleteAsync_DeletesAndCommits()
    {
        var index = new RecordingSearchIndex();
        var service = new DocumentIndexingService(
            [new StubExtractor(true, "unused")],
            index);
        var documentId = DocumentId.New();

        await service.DeleteAsync(documentId, CancellationToken.None);

        Assert.Equal(documentId, index.Deleted);
        Assert.Equal(1, index.CommitCalls);
    }

    [Fact]
    public async Task UnsupportedDocument_FailsClearlyWithoutIndexMutation()
    {
        var index = new RecordingSearchIndex();
        var service = new DocumentIndexingService(
            [new StubExtractor(false, "unused")],
            index);
        var document = CreateDocument("file:///C:/Data/file.bin");

        Assert.False(service.CanIndex(document));
        await Assert.ThrowsAsync<NotSupportedException>(
            () => service.IndexAsync(document, CancellationToken.None));

        Assert.Null(index.Upserted);
        Assert.Equal(0, index.CommitCalls);
    }

    private static DocumentRecord CreateDocument(string locator)
        => DocumentRecord.Restore(
            DocumentId.New(),
            SourceId.New(),
            locator,
            Path.GetFileName(new Uri(locator).LocalPath),
            42,
            createdAtUtc: null,
            modifiedAtUtc: new DateTimeOffset(2026, 10, 24, 9, 0, 0, TimeSpan.Zero),
            lastSeenAtUtc: new DateTimeOffset(2026, 10, 24, 9, 1, 0, TimeSpan.Zero),
            ScanRunId.New(),
            missingSinceUtc: null);

    private sealed class StubExtractor(
        bool canExtract,
        string text,
        string? title = null,
        string language = "und") : IContentExtractor
    {
        public int ExtractCalls { get; private set; }

        public bool CanExtract(DocumentRecord document) => canExtract;

        public Task<ExtractedContent> ExtractAsync(
            DocumentRecord document,
            CancellationToken cancellationToken)
        {
            ExtractCalls++;
            return Task.FromResult(
                new ExtractedContent(
                    text,
                    title,
                    "text/plain",
                    language,
                    "stub",
                    "1"));
        }
    }

    private sealed class RecordingSearchIndex : ISearchIndex
    {
        public SearchIndexDocument? Upserted { get; private set; }

        public DocumentId? Deleted { get; private set; }

        public int CommitCalls { get; private set; }

        public Task UpsertAsync(SearchIndexDocument document, CancellationToken cancellationToken)
        {
            Upserted = document;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(DocumentId documentId, CancellationToken cancellationToken)
        {
            Deleted = documentId;
            return Task.CompletedTask;
        }

        public Task CommitAsync(CancellationToken cancellationToken)
        {
            CommitCalls++;
            return Task.CompletedTask;
        }

        public Task<SearchResultPage> SearchAsync(
            SearchRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult(new SearchResultPage([], 0));

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
