using Sasd.Crawler.Spike.A2.Search;

namespace Sasd.Crawler.Spike.A2.Tests;

public sealed class LuceneSearchIndexTests : IAsyncLifetime
{
    private readonly string indexPath = Path.Combine(Path.GetTempPath(), "sasd-a2-tests", Guid.NewGuid().ToString("N"));

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        if (Directory.Exists(indexPath)) Directory.Delete(indexPath, recursive: true);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task IDX_001_Phrase_boolean_fuzzy_and_prefix_queries_return_expected_documents()
    {
        await using var index = new LuceneSearchIndex(indexPath);
        await index.UpsertAsync(Document("1", "Alpha", "the quick brown fox jumps", "en", "text"));
        await index.UpsertAsync(Document("2", "Beta", "the slow green turtle", "en", "office"));

        Assert.Single((await index.SearchAsync(new("\"quick brown\""))).Hits);
        Assert.Single((await index.SearchAsync(new("quick AND fox"))).Hits);
        Assert.Single((await index.SearchAsync(new("quik~1"))).Hits);
        Assert.Single((await index.SearchAsync(new("turt*"))).Hits);
    }

    [Fact]
    public async Task IDX_001_German_and_English_analyzers_apply_language_stemming()
    {
        await using var index = new LuceneSearchIndex(indexPath);
        await index.UpsertAsync(Document("de", "Bericht", "Die Häuser stehen am Fluss", "de", "text"));
        await index.UpsertAsync(Document("en", "Report", "Several houses beside rivers", "en", "text"));

        Assert.Contains((await index.SearchAsync(new("Haus"))).Hits, hit => hit.Id == "de");
        Assert.Contains((await index.SearchAsync(new("house"))).Hits, hit => hit.Id == "en");
    }

    [Fact]
    public async Task IDX_002_Update_and_delete_are_visible_and_do_not_leave_stale_content()
    {
        await using var index = new LuceneSearchIndex(indexPath);
        await index.UpsertAsync(Document("1", "First", "oldword", "en", "text"));
        await index.UpsertAsync(Document("1", "First", "newword", "en", "text"));

        Assert.Empty((await index.SearchAsync(new("oldword"))).Hits);
        Assert.Single((await index.SearchAsync(new("newword"))).Hits);
        await index.DeleteAsync("1");
        Assert.Empty((await index.SearchAsync(new("newword"))).Hits);
    }

    [Fact]
    public async Task SEA_003_Highlighting_and_category_facets_are_returned()
    {
        await using var index = new LuceneSearchIndex(indexPath);
        await index.UpsertAsync(Document("1", "One", "searchable needle content", "en", "text"));
        await index.UpsertAsync(Document("2", "Two", "another needle document", "en", "office"));

        var result = await index.SearchAsync(new("needle"));

        Assert.Equal(2, result.TotalHits);
        Assert.All(result.Hits, hit => Assert.Contains("<mark>", hit.Snippet));
        Assert.Equal(1, result.CategoryFacets["text"]);
        Assert.Equal(1, result.CategoryFacets["office"]);
        Assert.Single((await index.SearchAsync(new("needle", CategoryFilter: "office"))).Hits);
    }

    [Fact]
    public async Task IDX_005_Committed_index_can_be_reopened_after_writer_shutdown()
    {
        await using (var first = new LuceneSearchIndex(indexPath))
        {
            await first.UpsertAsync(Document("1", "Durable", "survives restart", "en", "text"));
            await first.CommitAsync();
        }

        await using var reopened = new LuceneSearchIndex(indexPath);
        Assert.Single((await reopened.SearchAsync(new("survives"))).Hits);
    }

    [Fact]
    public async Task REL_003_Concurrent_updates_are_serialized_without_lost_documents()
    {
        await using var index = new LuceneSearchIndex(indexPath);
        await Task.WhenAll(Enumerable.Range(0, 100).Select(i =>
            index.UpsertAsync(Document(i.ToString(), $"Document {i}", "sharedtoken", "en", "text"))));

        var result = await index.SearchAsync(new("sharedtoken", 200));
        Assert.Equal(100, result.TotalHits);
    }

    [Fact]
    public async Task IDX_001_Readers_remain_available_while_the_coordinated_writer_updates()
    {
        await using var index = new LuceneSearchIndex(indexPath);
        await index.UpsertAsync(Document("seed", "Seed", "available", "en", "text"));
        var writes = Task.WhenAll(Enumerable.Range(0, 25).Select(i =>
            index.UpsertAsync(Document(i.ToString(), $"Document {i}", "available", "en", "text"))));
        var reads = Task.WhenAll(Enumerable.Range(0, 10).Select(_ =>
            index.SearchAsync(new("available", 100))));

        await writes;
        var results = await reads;
        Assert.All(results, result => Assert.NotEmpty(result.Hits));
    }

    [Fact]
    public void SEC_004_Invalid_index_location_fails_explicitly()
    {
        var file = Path.Combine(indexPath, "not-a-directory");
        Directory.CreateDirectory(indexPath);
        File.WriteAllText(file, "occupied");

        Assert.ThrowsAny<Exception>(() => new LuceneSearchIndex(file));
    }

    private static SearchDocument Document(string id, string title, string content, string language, string category) =>
        new(id, title, content, language, category, DateTimeOffset.UtcNow);
}
