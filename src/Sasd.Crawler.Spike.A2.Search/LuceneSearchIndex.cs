using Lucene.Net.Analysis;
using Lucene.Net.Analysis.De;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace Sasd.Crawler.Spike.A2.Search;

public sealed class LuceneSearchIndex : ISearchIndex
{
    private const LuceneVersion Version = LuceneVersion.LUCENE_48;
    private readonly Lucene.Net.Store.Directory directory;
    private readonly Analyzer analyzer;
    private readonly IndexWriter writer;
    private readonly SemaphoreSlim writeGate = new(1, 1);
    private bool disposed;
    private bool writeFailed;

    public LuceneSearchIndex(string indexPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexPath);
        System.IO.Directory.CreateDirectory(indexPath);
        directory = FSDirectory.Open(new DirectoryInfo(indexPath));
        analyzer = CreateAnalyzer();
        writer = CreateWriter(directory, analyzer);
    }

    internal LuceneSearchIndex(Lucene.Net.Store.Directory directory)
    {
        this.directory = directory ?? throw new ArgumentNullException(nameof(directory));
        analyzer = CreateAnalyzer();
        writer = CreateWriter(directory, analyzer);
    }

    public async Task UpsertAsync(SearchDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ValidateDocument(document);
        await writeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            EnsureWritable();
            try { writer.UpdateDocument(new Term("id", document.Id), ToLuceneDocument(document)); }
            catch (IOException) { writeFailed = true; throw; }
        }
        finally
        {
            writeGate.Release();
        }
    }

    public async Task DeleteAsync(string documentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        await writeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            EnsureWritable();
            try { writer.DeleteDocuments(new Term("id", documentId)); }
            catch (IOException) { writeFailed = true; throw; }
        }
        finally
        {
            writeGate.Release();
        }
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await writeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            EnsureWritable();
            try { writer.Commit(); }
            catch (IOException) { writeFailed = true; throw; }
        }
        finally
        {
            writeGate.Release();
        }
    }

    public async Task<SearchResultPage> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Query))
            throw new ArgumentException("A query is required.", nameof(request));
        if (request.Limit is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(request), "Limit must be between 1 and 1000.");

        await writeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            EnsureWritable();
            try { writer.Commit(); }
            catch (IOException) { writeFailed = true; throw; }
            using var reader = DirectoryReader.Open(directory);
            var searcher = new IndexSearcher(reader);
            var parser = new MultiFieldQueryParser(Version, new[] { "title", "content_de", "content_en", "content_other" }, analyzer)
            {
                DefaultOperator = Operator.AND,
                AllowLeadingWildcard = false
            };
            Query query = parser.Parse(request.Query);
            if (!string.IsNullOrWhiteSpace(request.CategoryFilter))
            {
                query = new BooleanQuery
                {
                    { query, Occur.MUST },
                    { new TermQuery(new Term("category", request.CategoryFilter)), Occur.MUST }
                };
            }
            var topDocs = searcher.Search(query, request.Limit);
            var hits = new List<SearchHit>(topDocs.ScoreDocs.Length);
            var facets = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var document = searcher.Doc(scoreDoc.Doc);
                var category = document.Get("category");
                facets[category] = facets.GetValueOrDefault(category) + 1;
                hits.Add(new SearchHit(
                    document.Get("id"),
                    document.Get("title"),
                    Highlight(query, document),
                    scoreDoc.Score));
            }

            return new SearchResultPage(hits, facets, topDocs.TotalHits);
        }
        finally
        {
            writeGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        await writeGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (disposed) return;
            try
            {
                if (writeFailed) writer.Rollback();
                else
                {
                    try
                    {
                        writer.Commit();
                        writer.Dispose();
                    }
                    catch (IOException)
                    {
                        writeFailed = true;
                        writer.Rollback();
                        throw;
                    }
                }
            }
            finally
            {
                analyzer.Dispose();
                directory.Dispose();
                disposed = true;
            }
        }
        finally
        {
            writeGate.Release();
            writeGate.Dispose();
        }
    }

    private static Analyzer CreateAnalyzer() => new PerFieldAnalyzerWrapper(
        new StandardAnalyzer(Version),
        new Dictionary<string, Analyzer>
        {
            ["content_de"] = new GermanAnalyzer(Version),
            ["content_en"] = new EnglishAnalyzer(Version)
        });

    private static IndexWriter CreateWriter(Lucene.Net.Store.Directory directory, Analyzer analyzer) =>
        new(directory, new IndexWriterConfig(Version, analyzer) { OpenMode = OpenMode.CREATE_OR_APPEND });

    private static Document ToLuceneDocument(SearchDocument source)
    {
        var contentField = source.Language.ToLowerInvariant() switch
        {
            "de" or "de-de" => "content_de",
            "en" or "en-us" or "en-gb" => "content_en",
            _ => "content_other"
        };
        return new Document
        {
            new StringField("id", source.Id, Field.Store.YES),
            new TextField("title", source.Title, Field.Store.YES),
            new TextField(contentField, source.Content, Field.Store.YES),
            new StringField("content_field", contentField, Field.Store.YES),
            new StringField("language", source.Language, Field.Store.YES),
            new StringField("category", source.Category, Field.Store.YES),
            new Int64Field("modified_ticks", source.ModifiedAt.UtcTicks, Field.Store.YES)
        };
    }

    private string Highlight(Query query, Document document)
    {
        var field = document.Get("content_field");
        var content = document.Get(field);
        var highlighter = new Highlighter(new SimpleHTMLFormatter("<mark>", "</mark>"), new QueryScorer(query));
        using var tokenStream = analyzer.GetTokenStream(field, content);
        return highlighter.GetBestFragment(tokenStream, content) ?? Truncate(content, 180);
    }

    private static string Truncate(string value, int length) =>
        value.Length <= length ? value : string.Concat(value.AsSpan(0, length), "…");

    private static void ValidateDocument(SearchDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(document.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(document.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(document.Language);
        ArgumentException.ThrowIfNullOrWhiteSpace(document.Category);
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, this);

    private void EnsureWritable()
    {
        ThrowIfDisposed();
        if (writeFailed) throw new InvalidOperationException("The Lucene writer encountered an I/O failure and must be disposed and reopened.");
    }
}
