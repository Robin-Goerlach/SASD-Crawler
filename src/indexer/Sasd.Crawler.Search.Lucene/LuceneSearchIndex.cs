using Lucene.Net.Analysis;
using Lucene.Net.Analysis.De;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Sasd.Crawler.Application.Search;
using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Search.Lucene;

/// <summary>
/// Embedded Lucene.NET implementation of the rebuildable SASD-Crawler search index.
/// </summary>
/// <remarks>
/// This production adapter intentionally starts correctness-first. Reads and writes share one gate and each search
/// commits pending writes before opening a reader, which makes read-your-writes behavior deterministic. Near-real-
/// time reader reuse and batching are optimization work and must not alter the public consistency semantics.
/// </remarks>
public sealed class LuceneSearchIndex : ISearchIndex
{
    private const LuceneVersion Version = LuceneVersion.LUCENE_48;

    private const string IdField = "document_id";
    private const string SourceIdField = "source_id";
    private const string LocatorField = "canonical_locator";
    private const string TitleStoredField = "title_stored";
    private const string LanguageField = "language";
    private const string ModifiedTicksField = "modified_ticks";

    private static readonly string[] SearchFields =
    [
        "title_de", "content_de",
        "title_en", "content_en",
        "title_other", "content_other",
    ];

    private readonly Lucene.Net.Store.Directory directory;
    private readonly Analyzer analyzer;
    private readonly IndexWriter writer;
    private readonly SemaphoreSlim gate = new(1, 1);
    private bool disposed;
    private bool writeFailed;

    /// <summary>Creates or opens a persistent filesystem-backed Lucene index.</summary>
    public LuceneSearchIndex(
        string indexPath,
        SearchAnalyzerConfiguration analyzerConfiguration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexPath);
        ArgumentNullException.ThrowIfNull(analyzerConfiguration);

        var fullPath = Path.GetFullPath(indexPath);
        System.IO.Directory.CreateDirectory(fullPath);

        directory = FSDirectory.Open(new DirectoryInfo(fullPath));
        analyzer = CreateAnalyzer(analyzerConfiguration);
        writer = CreateWriter(directory, analyzer);
        AnalyzerConfigurationVersion = analyzerConfiguration.Version;
    }

    /// <summary>
    /// Creates an index over a caller-owned Lucene directory for deterministic tests.
    /// </summary>
    internal LuceneSearchIndex(
        Lucene.Net.Store.Directory directory,
        SearchAnalyzerConfiguration analyzerConfiguration)
    {
        this.directory = directory ?? throw new ArgumentNullException(nameof(directory));
        ArgumentNullException.ThrowIfNull(analyzerConfiguration);

        analyzer = CreateAnalyzer(analyzerConfiguration);
        writer = CreateWriter(directory, analyzer);
        AnalyzerConfigurationVersion = analyzerConfiguration.Version;
    }

    /// <summary>Gets the analyzer configuration version used by this adapter instance.</summary>
    public string AnalyzerConfigurationVersion { get; }

    /// <inheritdoc />
    public async Task UpsertAsync(SearchIndexDocument document, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            EnsureWritable();
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                writer.UpdateDocument(
                    new Term(IdField, document.DocumentId.ToString()),
                    ToLuceneDocument(document));
            }
            catch (IOException)
            {
                writeFailed = true;
                throw;
            }
        }
        finally
        {
            gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(DocumentId documentId, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            EnsureWritable();
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                writer.DeleteDocuments(new Term(IdField, documentId.ToString()));
            }
            catch (IOException)
            {
                writeFailed = true;
                throw;
            }
        }
        finally
        {
            gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            EnsureWritable();
            CommitWriter();
        }
        finally
        {
            gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<SearchResultPage> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            EnsureWritable();
            cancellationToken.ThrowIfCancellationRequested();

            // Correctness-first read-your-writes semantics: until a later measured NRT optimization exists, search
            // commits pending mutations and opens a fresh reader over a complete durable commit.
            CommitWriter();

            using var reader = DirectoryReader.Open(directory);
            var searcher = new IndexSearcher(reader);
            var parser = new MultiFieldQueryParser(Version, SearchFields, analyzer)
            {
                DefaultOperator = Operator.AND,
                AllowLeadingWildcard = false,
            };

            Query query = parser.Parse(request.QueryText);

            if (request.SourceId.HasValue)
            {
                query = new BooleanQuery
                {
                    { query, Occur.MUST },
                    { new TermQuery(new Term(SourceIdField, request.SourceId.Value.ToString())), Occur.MUST },
                };
            }

            var topDocs = searcher.Search(query, request.Limit);
            var hits = new List<SearchHit>(topDocs.ScoreDocs.Length);

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var document = searcher.Doc(scoreDoc.Doc);

                hits.Add(
                    new SearchHit(
                        DocumentId.Parse(document.Get(IdField)),
                        SourceId.Parse(document.Get(SourceIdField)),
                        document.Get(TitleStoredField),
                        document.Get(LocatorField),
                        scoreDoc.Score));
            }

            return new SearchResultPage(hits, topDocs.TotalHits);
        }
        finally
        {
            gate.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (disposed)
            {
                return;
            }

            try
            {
                if (writeFailed)
                {
                    // A write I/O failure may have left uncommitted segment state. Roll back to the last complete
                    // commit rather than attempting another implicit commit during disposal.
                    writer.Rollback();
                }
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
            gate.Release();
            gate.Dispose();
        }
    }

    private static Analyzer CreateAnalyzer(SearchAnalyzerConfiguration configuration)
    {
        var german = new GermanAnalyzer(
            Version,
            new CharArraySet(Version, configuration.GermanStopWords, ignoreCase: true));
        var english = new EnglishAnalyzer(
            Version,
            new CharArraySet(Version, configuration.EnglishStopWords, ignoreCase: true));

        return new PerFieldAnalyzerWrapper(
            new StandardAnalyzer(Version),
            new Dictionary<string, Analyzer>(StringComparer.Ordinal)
            {
                ["title_de"] = german,
                ["content_de"] = german,
                ["title_en"] = english,
                ["content_en"] = english,
            });
    }

    private static IndexWriter CreateWriter(
        Lucene.Net.Store.Directory directory,
        Analyzer analyzer)
        => new(
            directory,
            new IndexWriterConfig(Version, analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND,
            });

    private static Document ToLuceneDocument(SearchIndexDocument source)
    {
        var languageSuffix = NormalizeLanguageSuffix(source.Language);

        return new Document
        {
            new StringField(IdField, source.DocumentId.ToString(), Field.Store.YES),
            new StringField(SourceIdField, source.SourceId.ToString(), Field.Store.YES),
            new StringField(LocatorField, source.CanonicalLocator, Field.Store.YES),
            new StringField(TitleStoredField, source.Title, Field.Store.YES),
            new TextField($"title_{languageSuffix}", source.Title, Field.Store.NO),
            new TextField($"content_{languageSuffix}", source.Content, Field.Store.NO),
            new StringField(LanguageField, source.Language, Field.Store.YES),
            new Int64Field(ModifiedTicksField, source.ModifiedAtUtc.UtcTicks, Field.Store.YES),
        };
    }

    private void CommitWriter()
    {
        try
        {
            writer.Commit();
        }
        catch (IOException)
        {
            writeFailed = true;
            throw;
        }
    }

    private static string NormalizeLanguageSuffix(string language)
    {
        if (language.StartsWith("de", StringComparison.OrdinalIgnoreCase))
        {
            return "de";
        }

        if (language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            return "en";
        }

        return "other";
    }

    private void EnsureWritable()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        if (writeFailed)
        {
            throw new InvalidOperationException(
                "The Lucene writer encountered an I/O failure and must be disposed and reopened.");
        }
    }
}
