using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using Sasd.Crawler.Spike.A3.Tika;

if (args.Length < 2 || args[0] != "--jar")
    throw new ArgumentException("Usage: --jar <path> [--java <path>]");

var jarPath = Path.GetFullPath(args[1]);
var javaPath = args.Length >= 4 && args[2] == "--java" ? Path.GetFullPath(args[3]) : "java.exe";
if (!Path.IsPathFullyQualified(javaPath))
{
    javaPath = ResolveExecutable(javaPath) ?? throw new FileNotFoundException("Java was not found on PATH.");
}

var root = Path.Combine(Path.GetTempPath(), "sasd-a3-probe", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
var fixtures = FixtureFactory.Create(root);
var options = new TikaSidecarOptions
{
    JavaExecutablePath = javaPath,
    ServerJarPath = jarPath,
    TemporaryRootPath = Path.Combine(root, "sidecar-temp"),
    MaximumInputBytes = 10 * 1024 * 1024,
    MaximumHeapMegabytes = 512,
    StartupTimeout = TimeSpan.FromSeconds(45),
    RequestTimeout = TimeSpan.FromSeconds(15)
};

var durations = new Dictionary<string, double>();
var metadataCounts = new Dictionary<string, int>();
var watch = Stopwatch.StartNew();
await using (var sidecar = new TikaSidecar(options))
{
    await sidecar.StartAsync();
    watch.Stop();
    var startupMilliseconds = watch.Elapsed.TotalMilliseconds;
    if (sidecar.LoopbackEndpoint is null || !IPAddress.IsLoopback(IPAddress.Parse(sidecar.LoopbackEndpoint.Host)))
        throw new InvalidOperationException("Sidecar did not bind to an explicit loopback address.");
    var listeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()
        .Where(endpoint => endpoint.Port == sidecar.LoopbackEndpoint.Port)
        .ToArray();
    if (listeners.Length == 0 || listeners.Any(endpoint => !IPAddress.IsLoopback(endpoint.Address)))
        throw new InvalidOperationException("Tika listener is absent or exposed beyond loopback.");

    foreach (var fixture in fixtures.Valid)
    {
        watch.Restart();
        var result = await sidecar.ExtractAsync(fixture.Value);
        watch.Stop();
        if (!result.Text.Contains(FixtureFactory.Marker, StringComparison.Ordinal))
            throw new InvalidOperationException($"Marker missing from {fixture.Key} extraction.");
        durations[fixture.Key] = watch.Elapsed.TotalMilliseconds;
        metadataCounts[fixture.Key] = result.Metadata.Count;
    }

    var firstProcessId = sidecar.ProcessId;
    await sidecar.RestartAsync();
    var restartChangedProcess = firstProcessId != sidecar.ProcessId && sidecar.IsRunning;
    var postRestart = await sidecar.ExtractAsync(fixtures.Valid["docx"]);
    if (!postRestart.Text.Contains(FixtureFactory.Marker, StringComparison.Ordinal) || !restartChangedProcess)
        throw new InvalidOperationException("Restart verification failed.");

    var malformedRejected = false;
    try { _ = await sidecar.ExtractAsync(fixtures.MalformedPdf); }
    catch (TikaParserException) { malformedRejected = true; }

    var logs = sidecar.GetRecentLogs();
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        TikaJarBytes = new FileInfo(jarPath).Length,
        JavaVersion = GetJavaVersion(javaPath),
        StartupMilliseconds = startupMilliseconds,
        LoopbackEndpoint = sidecar.LoopbackEndpoint.ToString(),
        FormatDurationsMilliseconds = durations,
        MetadataFieldCounts = metadataCounts,
        RestartChangedProcess = restartChangedProcess,
        MalformedPdfRejected = malformedRejected,
        CapturedLogLines = logs.Count,
        IsRunningBeforeDispose = sidecar.IsRunning
    }, new JsonSerializerOptions { WriteIndented = true }));
}

await VerifyTimeoutAndSizeLimitAsync(options, fixtures.Valid["pdf"]);
Directory.Delete(root, recursive: true);

static async Task VerifyTimeoutAndSizeLimitAsync(TikaSidecarOptions options, string input)
{
    await using (var limited = new TikaSidecar(options with { MaximumInputBytes = 1 }))
    {
        await AssertThrowsAsync<TikaInputTooLargeException>(() => limited.ExtractAsync(input));
        if (limited.IsRunning) throw new InvalidOperationException("Size rejection started the sidecar.");
    }

    await using var timed = new TikaSidecar(options with { RequestTimeout = TimeSpan.FromTicks(1) });
    await AssertThrowsAsync<OperationCanceledException>(() => timed.ExtractAsync(input));
    if (timed.IsRunning) throw new InvalidOperationException("Timed-out parser process was not stopped.");
}

static async Task AssertThrowsAsync<TException>(Func<Task> action) where TException : Exception
{
    try { await action(); }
    catch (TException) { return; }
    throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
}

static string? ResolveExecutable(string name) =>
    (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
        .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
        .Select(path => Path.Combine(path, name))
        .FirstOrDefault(File.Exists);

static string GetJavaVersion(string javaPath)
{
    var info = new ProcessStartInfo(javaPath, "-version") { UseShellExecute = false, RedirectStandardError = true, CreateNoWindow = true };
    using var process = Process.Start(info)!;
    var version = process.StandardError.ReadLine() ?? "unknown";
    process.WaitForExit();
    return version;
}

internal static class FixtureFactory
{
    public const string Marker = "SASD_A3_MARKER_2026";

    public static (Dictionary<string, string> Valid, string MalformedPdf) Create(string root)
    {
        var valid = new Dictionary<string, string>
        {
            ["docx"] = CreateDocx(Path.Combine(root, "sample.docx")),
            ["xlsx"] = CreateXlsx(Path.Combine(root, "sample.xlsx")),
            ["pptx"] = CreatePptx(Path.Combine(root, "sample.pptx")),
            ["pdf"] = CreatePdf(Path.Combine(root, "sample.pdf"))
        };
        var malformed = Path.Combine(root, "malformed.pdf");
        File.WriteAllText(malformed, "%PDF-1.7\nthis is intentionally malformed");
        return (valid, malformed);
    }

    private static string CreateDocx(string path)
    {
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        Add(archive, "[Content_Types].xml", "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/></Types>");
        Add(archive, "_rels/.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/></Relationships>");
        Add(archive, "word/document.xml", $"<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body><w:p><w:r><w:t>{Marker}</w:t></w:r></w:p></w:body></w:document>");
        return path;
    }

    private static string CreateXlsx(string path)
    {
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        Add(archive, "[Content_Types].xml", "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
        Add(archive, "_rels/.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
        Add(archive, "xl/workbook.xml", "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"Sheet1\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
        Add(archive, "xl/_rels/workbook.xml.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");
        Add(archive, "xl/worksheets/sheet1.xml", $"<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData><row r=\"1\"><c r=\"A1\" t=\"inlineStr\"><is><t>{Marker}</t></is></c></row></sheetData></worksheet>");
        return path;
    }

    private static string CreatePptx(string path)
    {
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        Add(archive, "[Content_Types].xml", "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Override PartName=\"/ppt/presentation.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml\"/><Override PartName=\"/ppt/slides/slide1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.presentationml.slide+xml\"/></Types>");
        Add(archive, "_rels/.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"ppt/presentation.xml\"/></Relationships>");
        Add(archive, "ppt/presentation.xml", "<p:presentation xmlns:p=\"http://schemas.openxmlformats.org/presentationml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><p:sldIdLst><p:sldId id=\"256\" r:id=\"rId1\"/></p:sldIdLst></p:presentation>");
        Add(archive, "ppt/_rels/presentation.xml.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide\" Target=\"slides/slide1.xml\"/></Relationships>");
        Add(archive, "ppt/slides/slide1.xml", $"<p:sld xmlns:p=\"http://schemas.openxmlformats.org/presentationml/2006/main\" xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\"><p:cSld><p:spTree><p:sp><p:txBody><a:bodyPr/><a:lstStyle/><a:p><a:r><a:t>{Marker}</a:t></a:r></a:p></p:txBody></p:sp></p:spTree></p:cSld></p:sld>");
        return path;
    }

    private static string CreatePdf(string path)
    {
        var objects = new[]
        {
            "1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n",
            "2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n",
            "3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 612 792]/Resources<</Font<</F1 4 0 R>>>>/Contents 5 0 R>>endobj\n",
            "4 0 obj<</Type/Font/Subtype/Type1/BaseFont/Helvetica>>endobj\n",
            $"5 0 obj<</Length {Marker.Length + 31}>>stream\nBT /F1 12 Tf 72 720 Td ({Marker}) Tj ET\nendstream\nendobj\n"
        };
        var builder = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        foreach (var item in objects) { offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString())); builder.Append(item); }
        var xref = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.Append($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1)) builder.Append($"{offset:0000000000} 00000 n \n");
        builder.Append($"trailer<</Size {objects.Length + 1}/Root 1 0 R>>\nstartxref\n{xref}\n%%EOF");
        File.WriteAllText(path, builder.ToString(), Encoding.ASCII);
        return path;
    }

    private static void Add(ZipArchive archive, string name, string content)
    {
        var entry = archive.CreateEntry(name);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }
}
