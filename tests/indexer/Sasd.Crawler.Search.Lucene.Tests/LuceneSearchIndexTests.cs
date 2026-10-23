using Sasd.Crawler.Application.Search;
using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Search.Lucene.Tests;

/// <summary>
/// Verifies the production Lucene adapter against a real filesystem index.
/// </summary>
public sealed class LuceneSearchIndexTests : IAsyncLifetime
{
    private readonly string indexPath = Path.Combine(
        Path.GetTempPath(),
        "SasdCrawlerLuceneTests",
        Guid.NewGuid().ToString("N"));

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        if (Directory.Exists(indexPath))
        {
            Directory.Delete(indexPath, recursive: true);
        }

        return Task.CompletedTask;
    }

    [Fact]
    public async Task Idx001_KeywordSearch_FindsIndexedTitleAndContent()
    {
        await using var index = CreateIndex();

        var alpha = Document(
            "Alpha report",
            "The production crawler contains a searchable architecture note.",
            "en");
        var beta = Document(
            "Beta",
            "Completely unrelated material.",
            "en");

        await index.UpsertAsync(alpha, CancellationToken.None);
        await index.UpsertAsync(beta, CancellationToken.None);

        var result = await index.SearchAsync(
            new SearchRequest("searchable architecture"),
            CancellationToken.None);

        var hit = Assert.Single(result.Hits);
        Assert.Equal(alpha.DocumentId, hit.DocumentId);
        Assert.Equal(alpha.CanonicalLocator, hit.CanonicalLocator);
    }

    [Fact]
    public async Task Idx002_Upsert_ReplacesStaleContentForSameDocumentId()
    {
        await using var index = CreateIndex();
        var id = DocumentId.New();
        var sourceId = SourceId.New();

        await index.UpsertAsync(
            Document("First", "oldtoken", "en", id, sourceId),
            CancellationToken.None);
        await index.UpsertAsync(
            Document("First", "newtoken", "en", id, sourceId),
            CancellationToken.None);

        Assert.Empty((await index.SearchAsync(
            new SearchRequest("oldtoken"),
            CancellationToken.None)).Hits);
        Assert.Single((await index.SearchAsync(
            new SearchRequest("newtoken"),
            CancellationToken.None)).Hits);
    }

    [Fact]
    public async Task Idx003_Delete_RemovesOnlyRequestedDocument()
    {
        await using var index = CreateIndex();
        var first = Document("One", "shared deletiontoken", "en");
        var second = Document("Two", "shared keepstoken", "en");

        await index.UpsertAsync(first, CancellationToken.None);
        await index.UpsertAsync(second, CancellationToken.None);
        await index.DeleteAsync(first.DocumentId, CancellationToken.None);

        Assert.Empty((await index.SearchAsync(
            new SearchRequest("deletiontoken"),
            CancellationToken.None)).Hits);
        Assert.Single((await index.SearchAsync(
            new SearchRequest("keepstoken"),
            CancellationToken.None)).Hits);
    }

    [Fact]
    public async Task Idx001_SourceFilter_ReturnsOnlyDocumentsFromRequestedSource()
    {
        await using var index = CreateIndex();
        var sourceA = SourceId.New();
        var sourceB = SourceId.New();

        await index.UpsertAsync(
            Document("A", "sharedtoken", "en", sourceId: sourceA),
            CancellationToken.None);
        await index.UpsertAsync(
            Document("B", "sharedtoken", "en", sourceId: sourceB),
            CancellationToken.None);

        var result = await index.SearchAsync(
            new SearchRequest("sharedtoken", sourceId: sourceB),
            CancellationToken.None);

        var hit = Assert.Single(result.Hits);
        Assert.Equal(sourceB, hit.SourceId);
    }

    [Fact]
    public async Task Idx001_GermanAndEnglishAnalyzers_ApplyLanguageStemming()
    {
        await using var index = CreateIndex();

        var german = Document("Bericht", "Die Häuser stehen am Fluss.", "de");
        var english = Document("Report", "Several houses stand beside rivers.", "en");

        await index.UpsertAsync(german, CancellationToken.None);
        await index.UpsertAsync(english, CancellationToken.None);

        Assert.Contains(
            (await index.SearchAsync(new SearchRequest("Haus"), CancellationToken.None)).Hits,
            hit => hit.DocumentId == german.DocumentId);
        Assert.Contains(
            (await index.SearchAsync(new SearchRequest("house"), CancellationToken.None)).Hits,
            hit => hit.DocumentId == english.DocumentId);
    }

    [Fact]
    public async Task Sea022_DefaultStopWords_FilterGermanArticlesButKeepJa()
    {
        await using var index = CreateIndex();
        var document = Document("Antwort", "der status ist ja bestätigt", "de");

        await index.UpsertAsync(document, CancellationToken.None);

        Assert.Empty((await index.SearchAsync(
            new SearchRequest("der"),
            CancellationToken.None)).Hits);
        Assert.Single((await index.SearchAsync(
            new SearchRequest("ja"),
            CancellationToken.None)).Hits);
    }

    [Fact]
    public async Task Sea022_CustomStopWords_AreUsedForIndexAndQueryAnalysis()
    {
        var defaults = SearchAnalyzerConfiguration.CreateDefault();
        var customized = new SearchAnalyzerConfiguration(
            "custom-test-v1",
            defaults.GermanStopWords.Concat(["ja"]),
            defaults.EnglishStopWords);

        await using var index = new LuceneSearchIndex(indexPath, customized);
        await index.UpsertAsync(
            Document("Antwort", "ja bestätigt", "de"),
            CancellationToken.None);

        Assert.Equal("custom-test-v1", index.AnalyzerConfigurationVersion);
        Assert.Empty((await index.SearchAsync(
            new SearchRequest("ja"),
            CancellationToken.None)).Hits);
    }

    [Fact]
    public async Task Idx001_CommittedIndex_CanBeReopened()
    {
        var document = Document("Durable", "survives restart", "en");

        await using (var first = CreateIndex())
        {
            await first.UpsertAsync(document, CancellationToken.None);
            await first.CommitAsync(CancellationToken.None);
        }

        await using var reopened = CreateIndex();
        var result = await reopened.SearchAsync(
            new SearchRequest("survives"),
            CancellationToken.None);

        Assert.Contains(result.Hits, hit => hit.DocumentId == document.DocumentId);
    }

    [Fact]
    public async Task ConcurrentUpserts_AreSerializedWithoutLosingDocuments()
    {
        await using var index = CreateIndex();
        var sourceId = SourceId.New();

        await Task.WhenAll(
            Enumerable.Range(0, 50)
                .Select(i => index.UpsertAsync(
                    Document($"Document {i}", "concurrencytoken", "en", sourceId: sourceId),
                    CancellationToken.None)));

        var result = await index.SearchAsync(
            new SearchRequest("concurrencytoken", limit: 100),
            CancellationToken.None);

        Assert.Equal(50, result.TotalHits);
    }

    [Fact]
    public void InvalidIndexLocation_FailsExplicitly()
    {
        Directory.CreateDirectory(indexPath);
        var occupiedFile = Path.Combine(indexPath, "not-a-directory");
        File.WriteAllText(occupiedFile, "occupied");

        Assert.ThrowsAny<Exception>(
            () => new LuceneSearchIndex(
                occupiedFile,
                SearchAnalyzerConfiguration.CreateDefault()));
    }

    private LuceneSearchIndex CreateIndex()
        => new(indexPath, SearchAnalyzerConfiguration.CreateDefault());

    private static SearchIndexDocument Document(
        string title,
        string content,
        string language,
        DocumentId? documentId = null,
        SourceId? sourceId = null)
    {
        var id = documentId ?? DocumentId.New();

        return new SearchIndexDocument(
            id,
            sourceId ?? SourceId.New(),
            $"file:///C:/Data/{id.Value:N}.txt",
            title,
            content,
            language,
            new DateTimeOffset(2026, 10, 23, 12, 0, 0, TimeSpan.Zero));
    }
}
