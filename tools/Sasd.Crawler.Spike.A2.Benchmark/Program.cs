using System.Diagnostics;
using System.Text.Json;
using Sasd.Crawler.Spike.A2.Search;

var options = BenchmarkOptions.Parse(args);
System.IO.Directory.CreateDirectory(options.IndexPath);
var corpus = options.CorpusPath is null ? null : LoadCorpus(options.CorpusPath);
var documentCount = corpus?.Count ?? options.DocumentCount;
SearchDocument GetDocument(int index) => corpus is null ? CreateDocument(index) : corpus[index];

if (options.CrashAfter is not null)
{
    await WriteUntilCrashAsync(options);
    return;
}

var process = Process.GetCurrentProcess();
var started = Stopwatch.StartNew();
await using (var index = new LuceneSearchIndex(options.IndexPath))
{
    for (var i = 0; i < documentCount; i++)
    {
        await index.UpsertAsync(GetDocument(i));
    }

    await index.CommitAsync();
}
started.Stop();
var initialIndexBytes = System.IO.Directory.EnumerateFiles(options.IndexPath).Sum(file => new FileInfo(file).Length);

var mutationCount = Math.Min(Math.Max(1, documentCount / 10), 1_000);
var updateWatch = Stopwatch.StartNew();
await using (var index = new LuceneSearchIndex(options.IndexPath))
{
    for (var i = 0; i < mutationCount; i++)
    {
        var original = GetDocument(i);
        await index.UpsertAsync(original with { Content = original.Content + " updated" });
    }
    await index.CommitAsync();
}
updateWatch.Stop();

var deleteWatch = Stopwatch.StartNew();
await using (var index = new LuceneSearchIndex(options.IndexPath))
{
    for (var i = 0; i < mutationCount; i++)
    {
        await index.DeleteAsync(GetDocument(i).Id);
    }
    await index.CommitAsync();
}
deleteWatch.Stop();

var queryDurations = new List<double>(options.QueryCount);
var minimumQueryHits = long.MaxValue;
await using (var index = new LuceneSearchIndex(options.IndexPath))
{
    for (var i = 0; i < options.QueryCount; i++)
    {
        var queryWatch = Stopwatch.StartNew();
        var queryResult = await index.SearchAsync(new SearchRequest(i % 2 == 0 ? "architecture OR Architektur" : "crawler OR Crawler", 20));
        minimumQueryHits = Math.Min(minimumQueryHits, queryResult.TotalHits);
        queryWatch.Stop();
        queryDurations.Add(queryWatch.Elapsed.TotalMilliseconds);
    }
}
if (minimumQueryHits == 0) throw new InvalidOperationException("At least one benchmark query returned no hits.");

queryDurations.Sort();
var result = new
{
    DocumentCount = documentCount,
    CorpusPath = options.CorpusPath,
    IndexingSeconds = started.Elapsed.TotalSeconds,
    DocumentsPerSecond = documentCount / started.Elapsed.TotalSeconds,
    IndexBytes = initialIndexBytes,
    BytesPerDocument = (double)initialIndexBytes / documentCount,
    UpdateMillisecondsPerDocument = updateWatch.Elapsed.TotalMilliseconds / mutationCount,
    DeleteMillisecondsPerDocument = deleteWatch.Elapsed.TotalMilliseconds / mutationCount,
    QueryP50Milliseconds = Percentile(queryDurations, 0.50),
    QueryP95Milliseconds = Percentile(queryDurations, 0.95),
    MinimumQueryHits = minimumQueryHits,
    PeakWorkingSetBytes = process.PeakWorkingSet64,
    Runtime = Environment.Version.ToString(),
    OS = Environment.OSVersion.ToString()
};
Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

static async Task WriteUntilCrashAsync(BenchmarkOptions options)
{
    await using var index = new LuceneSearchIndex(options.IndexPath);
    for (var i = 0; i < options.CrashAfter; i++)
    {
        await index.UpsertAsync(CreateDocument(i));
    }

    Environment.FailFast("Intentional A2 crash-recovery probe after uncommitted writes.");
}

static SearchDocument CreateDocument(int i) => new(
    i.ToString(),
    $"Synthetic crawler document {i}",
    $"This searchable architecture document number {i} validates indexing, update and recovery behavior. Token{i % 1000}.",
    i % 3 == 0 ? "de" : "en",
    i % 5 == 0 ? "office" : "text",
    DateTimeOffset.UnixEpoch.AddSeconds(i));

static IReadOnlyList<SearchDocument> LoadCorpus(string corpusPath)
{
    var root = Path.GetFullPath(corpusPath);
    if (!System.IO.Directory.Exists(root)) throw new DirectoryNotFoundException(root);
    var files = System.IO.Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories)
        .Concat(System.IO.Directory.EnumerateFiles(root, "*.txt", SearchOption.AllDirectories))
        .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
        .ToArray();
    if (files.Length == 0) throw new InvalidOperationException("Corpus contains no .md or .txt files.");
    return files.Select(path => new SearchDocument(
        Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'),
        Path.GetFileNameWithoutExtension(path),
        File.ReadAllText(path),
        "de",
        Path.GetExtension(path).TrimStart('.').ToLowerInvariant(),
        new DateTimeOffset(File.GetLastWriteTimeUtc(path), TimeSpan.Zero))).ToArray();
}

static double Percentile(IReadOnlyList<double> sorted, double percentile) =>
    sorted[(int)Math.Ceiling(percentile * sorted.Count) - 1];

internal sealed record BenchmarkOptions(string IndexPath, int DocumentCount, int QueryCount, int? CrashAfter, string? CorpusPath)
{
    public static BenchmarkOptions Parse(string[] args)
    {
        string? path = null;
        var documents = 10_000;
        var queries = 100;
        int? crashAfter = null;
        string? corpusPath = null;
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--index": path = args[++i]; break;
                case "--documents": documents = int.Parse(args[++i]); break;
                case "--queries": queries = int.Parse(args[++i]); break;
                case "--crash-after": crashAfter = int.Parse(args[++i]); break;
                case "--corpus": corpusPath = Path.GetFullPath(args[++i]); break;
                default: throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }

        if (documents < 1 || queries < 1) throw new ArgumentOutOfRangeException(nameof(args));
        path ??= Path.Combine(Path.GetTempPath(), "sasd-a2-benchmark", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        return new BenchmarkOptions(Path.GetFullPath(path), documents, queries, crashAfter, corpusPath);
    }
}
