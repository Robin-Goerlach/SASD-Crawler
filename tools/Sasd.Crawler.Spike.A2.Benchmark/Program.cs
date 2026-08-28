using System.Diagnostics;
using System.Text.Json;
using Sasd.Crawler.Spike.A2.Search;

var options = BenchmarkOptions.Parse(args);
System.IO.Directory.CreateDirectory(options.IndexPath);

if (options.CrashAfter is not null)
{
    await WriteUntilCrashAsync(options);
    return;
}

var process = Process.GetCurrentProcess();
var started = Stopwatch.StartNew();
await using (var index = new LuceneSearchIndex(options.IndexPath))
{
    for (var i = 0; i < options.DocumentCount; i++)
    {
        await index.UpsertAsync(CreateDocument(i));
    }

    await index.CommitAsync();
}
started.Stop();

var mutationCount = Math.Min(options.DocumentCount, 1_000);
var updateWatch = Stopwatch.StartNew();
await using (var index = new LuceneSearchIndex(options.IndexPath))
{
    for (var i = 0; i < mutationCount; i++)
    {
        var original = CreateDocument(i);
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
        await index.DeleteAsync(i.ToString());
    }
    await index.CommitAsync();
}
deleteWatch.Stop();

var queryDurations = new List<double>(options.QueryCount);
await using (var index = new LuceneSearchIndex(options.IndexPath))
{
    for (var i = 0; i < options.QueryCount; i++)
    {
        var queryWatch = Stopwatch.StartNew();
        _ = await index.SearchAsync(new SearchRequest(i % 2 == 0 ? "architecture" : "crawler AND document", 20));
        queryWatch.Stop();
        queryDurations.Add(queryWatch.Elapsed.TotalMilliseconds);
    }
}

queryDurations.Sort();
var bytes = System.IO.Directory.EnumerateFiles(options.IndexPath).Sum(file => new FileInfo(file).Length);
var result = new
{
    options.DocumentCount,
    IndexingSeconds = started.Elapsed.TotalSeconds,
    DocumentsPerSecond = options.DocumentCount / started.Elapsed.TotalSeconds,
    IndexBytes = bytes,
    BytesPerDocument = (double)bytes / options.DocumentCount,
    UpdateMillisecondsPerDocument = updateWatch.Elapsed.TotalMilliseconds / mutationCount,
    DeleteMillisecondsPerDocument = deleteWatch.Elapsed.TotalMilliseconds / mutationCount,
    QueryP50Milliseconds = Percentile(queryDurations, 0.50),
    QueryP95Milliseconds = Percentile(queryDurations, 0.95),
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

static double Percentile(IReadOnlyList<double> sorted, double percentile) =>
    sorted[(int)Math.Ceiling(percentile * sorted.Count) - 1];

internal sealed record BenchmarkOptions(string IndexPath, int DocumentCount, int QueryCount, int? CrashAfter)
{
    public static BenchmarkOptions Parse(string[] args)
    {
        string? path = null;
        var documents = 10_000;
        var queries = 100;
        int? crashAfter = null;
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--index": path = args[++i]; break;
                case "--documents": documents = int.Parse(args[++i]); break;
                case "--queries": queries = int.Parse(args[++i]); break;
                case "--crash-after": crashAfter = int.Parse(args[++i]); break;
                default: throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }

        if (documents < 1 || queries < 1) throw new ArgumentOutOfRangeException(nameof(args));
        path ??= Path.Combine(Path.GetTempPath(), "sasd-a2-benchmark", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        return new BenchmarkOptions(Path.GetFullPath(path), documents, queries, crashAfter);
    }
}
