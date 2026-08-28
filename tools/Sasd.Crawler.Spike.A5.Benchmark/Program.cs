using System.Diagnostics;
using System.Text.Json;
using Sasd.Crawler.Spike.A3.Tika;
using NPOI.HSSF.UserModel;
using Toxy;

if (args.Length < 2 || args[0] != "--jar") throw new ArgumentException("Usage: --jar <path> [--java <path>]");
var jar = Path.GetFullPath(args[1]);
var java = args.Length >= 4 && args[2] == "--java" ? Path.GetFullPath(args[3]) : ResolveExecutable("java.exe")!;
var root = Path.Combine(Path.GetTempPath(), "sasd-a5-benchmark", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
var baseFixtures = FixtureFactory.Create(root);
var fixtures = baseFixtures.Valid;
fixtures["html"] = Write("sample.html", $"<!doctype html><html><head><title>A5</title></head><body>{FixtureFactory.Marker}</body></html>");
fixtures["rtf"] = Write("sample.rtf", $@"{{\rtf1\ansi\deff0 {{\fonttbl {{\f0 Arial;}}}}\f0\fs24 {FixtureFactory.Marker}\par}}");
fixtures["txt"] = Write("sample.txt", FixtureFactory.Marker);
fixtures["xls"] = CreateLegacyXls(Path.Combine(root, "sample.xls"));
fixtures["large-txt"] = Write("large.txt", new string('L', 5 * 1024 * 1024) + FixtureFactory.Marker);

var tikaOptions = new TikaSidecarOptions
{
    JavaExecutablePath = java,
    ServerJarPath = jar,
    TemporaryRootPath = Path.Combine(root, "tika-temp"),
    RequestTimeout = TimeSpan.FromSeconds(20),
    MaximumInputBytes = 10 * 1024 * 1024
};

var rows = new List<BenchmarkRow>();
await using (var tika = new TikaSidecar(tikaOptions))
{
    await tika.StartAsync();
    foreach (var fixture in fixtures)
    {
        rows.Add(await MeasureTikaAsync(tika, fixture.Key, fixture.Value));
        rows.Add(MeasureToxy(fixture.Key, fixture.Value));
    }

    rows.Add(await MeasureFailureAsync("malformed-pdf", "Tika", () => tika.ExtractAsync(baseFixtures.MalformedPdf)));
    rows.Add(MeasureToxy("malformed-pdf", baseFixtures.MalformedPdf));
}

Console.WriteLine(JsonSerializer.Serialize(new
{
    ToxyVersion = typeof(ParserFactory).Assembly.GetName().Version?.ToString(),
    TikaJarBytes = new FileInfo(jar).Length,
    Results = rows,
    ArchitecturalObservations = new
    {
        Tika = "Out-of-process; timeout kills sidecar; async stream API; 512 MiB configured heap cap.",
        Toxy = "In-process synchronous Parse(); no cooperative CancellationToken or process boundary."
    }
}, new JsonSerializerOptions { WriteIndented = true }));

GC.Collect();
GC.WaitForPendingFinalizers();
Directory.Delete(root, recursive: true);
return;

string Write(string name, string content)
{
    var path = Path.Combine(root, name);
    File.WriteAllText(path, content);
    return path;
}

static async Task<BenchmarkRow> MeasureTikaAsync(TikaSidecar tika, string format, string path)
{
    var watch = Stopwatch.StartNew();
    try
    {
        var result = await tika.ExtractAsync(path);
        watch.Stop();
        return new(format, "Tika", true, result.Text.Contains(FixtureFactory.Marker, StringComparison.Ordinal),
            result.Text.Length, result.Metadata.Count, watch.Elapsed.TotalMilliseconds, GetWorkingSet(tika.ProcessId), null, null);
    }
    catch (Exception exception)
    {
        watch.Stop();
        return new(format, "Tika", false, false, 0, 0, watch.Elapsed.TotalMilliseconds, GetWorkingSet(tika.ProcessId), $"{exception.GetType().Name}: {exception.Message}", null);
    }
}

static BenchmarkRow MeasureToxy(string format, string path)
{
    var watch = Stopwatch.StartNew();
    try
    {
        var text = ParserFactory.CreateText(new ParserContext(path)).Parse();
        var metadataCount = 0;
        string? metadataError = null;
        try { metadataCount = ParserFactory.CreateMetadata(new ParserContext(path)).Parse().Count; }
        catch (Exception exception) { metadataError = $"{exception.GetType().Name}: {exception.Message}"; }
        watch.Stop();
        return new(format, "Toxy", true, text.Contains(FixtureFactory.Marker, StringComparison.Ordinal), text.Length,
            metadataCount, watch.Elapsed.TotalMilliseconds, Process.GetCurrentProcess().WorkingSet64, null, metadataError);
    }
    catch (Exception exception)
    {
        watch.Stop();
        return new(format, "Toxy", false, false, 0, 0, watch.Elapsed.TotalMilliseconds, Process.GetCurrentProcess().WorkingSet64, $"{exception.GetType().Name}: {exception.Message}", null);
    }
}

static async Task<BenchmarkRow> MeasureFailureAsync(string format, string parser, Func<Task<TikaExtractionResult>> action)
{
    var watch = Stopwatch.StartNew();
    try { _ = await action(); return new(format, parser, true, false, 0, 0, watch.Elapsed.TotalMilliseconds, 0, "UnexpectedSuccess", null); }
    catch (Exception exception) { return new(format, parser, false, false, 0, 0, watch.Elapsed.TotalMilliseconds, 0, $"{exception.GetType().Name}: {exception.Message}", null); }
}

static long GetWorkingSet(int? processId)
{
    if (processId is null) return 0;
    using var process = Process.GetProcessById(processId.Value);
    return process.WorkingSet64;
}

static string CreateLegacyXls(string path)
{
    using var workbook = new HSSFWorkbook();
    var sheet = workbook.CreateSheet("A5 Legacy");
    sheet.CreateRow(0).CreateCell(0).SetCellValue(FixtureFactory.Marker);
    using var output = File.Create(path);
    workbook.Write(output, leaveOpen: false);
    return path;
}

static string? ResolveExecutable(string name) => (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
    .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).Select(path => Path.Combine(path, name)).FirstOrDefault(File.Exists);

internal sealed record BenchmarkRow(string Format, string Parser, bool ParseSucceeded, bool MarkerComplete,
    int TextCharacters, int MetadataFields, double ElapsedMilliseconds, long WorkingSetBytesAfter, string? Error, string? MetadataError);
