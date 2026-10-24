using System.Text;
using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Extraction.Native;

namespace Sasd.Crawler.Extraction.Native.Tests;

/// <summary>
/// Exercises the native extractor against real disposable files.
/// </summary>
public sealed class NativeTextHtmlContentExtractorTests : IDisposable
{
    private readonly string directory = Path.Combine(
        Path.GetTempPath(),
        "SasdCrawlerNativeExtractionTests",
        Guid.NewGuid().ToString("N"));

    public NativeTextHtmlContentExtractorTests()
    {
        Directory.CreateDirectory(directory);
    }

    [Fact]
    public async Task Ext001_TextFile_ExtractsUtf8Content()
    {
        var path = WriteFile("note.txt", "Grüße aus dem SASD Crawler.");
        var extractor = CreateExtractor();

        var result = await extractor.ExtractAsync(Document(path), CancellationToken.None);

        Assert.Equal("Grüße aus dem SASD Crawler.", result.Text);
        Assert.Equal("text/plain", result.MediaType);
        Assert.Equal("und", result.Language);
        Assert.Null(result.Title);
    }

    [Fact]
    public async Task Ext001_TextFile_HonorsUtf16Bom()
    {
        var path = Path.Combine(directory, "unicode.txt");
        await File.WriteAllTextAsync(path, "Unicode Häuser", Encoding.Unicode);
        var extractor = CreateExtractor();

        var result = await extractor.ExtractAsync(Document(path), CancellationToken.None);

        Assert.Equal("Unicode Häuser", result.Text);
    }

    [Fact]
    public async Task Ext002_Html_ExtractsVisibleTextTitleAndLanguage()
    {
        var html = """
            <!doctype html>
            <html lang="de-DE">
              <head>
                <title>  Beispielseite  </title>
                <style>.secret { display:none; }</style>
                <script>const hiddenToken = "do-not-index";</script>
              </head>
              <body>
                <h1>Hallo &amp; willkommen</h1>
                <p>Text   mit
                   Leerraum.</p>
                <noscript>fallback-hidden</noscript>
                <template>template-hidden</template>
              </body>
            </html>
            """;
        var path = WriteFile("page.html", html);
        var extractor = CreateExtractor();

        var result = await extractor.ExtractAsync(Document(path), CancellationToken.None);

        Assert.Equal("Beispielseite", result.Title);
        Assert.Equal("de-de", result.Language);
        Assert.Equal("text/html", result.MediaType);
        Assert.Contains("Hallo & willkommen", result.Text, StringComparison.Ordinal);
        Assert.Contains("Text mit Leerraum.", result.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("do-not-index", result.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("fallback-hidden", result.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("template-hidden", result.Text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("file.pdf")]
    [InlineData("image.jpg")]
    [InlineData("document.docx")]
    public void UnsupportedExtension_IsNotClaimed(string fileName)
    {
        var path = WriteFile(fileName, "content");
        var extractor = CreateExtractor();

        Assert.False(extractor.CanExtract(Document(path)));
    }

    [Fact]
    public async Task SizeLimit_FailsBeforeParsingLargeInput()
    {
        var path = WriteFile("large.txt", "0123456789");
        var extractor = new NativeTextHtmlContentExtractor(
            new NativeContentExtractionOptions { MaximumInputBytes = 4 });

        var exception = await Assert.ThrowsAsync<ContentExtractionLimitExceededException>(
            () => extractor.ExtractAsync(Document(path), CancellationToken.None));

        Assert.Equal(10, exception.Length);
        Assert.Equal(4, exception.Limit);
    }

    [Fact]
    public async Task NonFileUri_IsRejected()
    {
        var extractor = CreateExtractor();
        var document = DocumentRecord.Restore(
            DocumentId.New(),
            SourceId.New(),
            "https://example.invalid/page.html",
            "page.html",
            100,
            createdAtUtc: null,
            modifiedAtUtc: DateTimeOffset.UtcNow,
            lastSeenAtUtc: DateTimeOffset.UtcNow,
            ScanRunId.New(),
            missingSinceUtc: null);

        Assert.False(extractor.CanExtract(document));
        await Assert.ThrowsAsync<NotSupportedException>(
            () => extractor.ExtractAsync(document, CancellationToken.None));
    }

    private NativeTextHtmlContentExtractor CreateExtractor()
        => new(new NativeContentExtractionOptions());

    private string WriteFile(string fileName, string content)
    {
        var path = Path.Combine(directory, fileName);
        File.WriteAllText(path, content, new UTF8Encoding(false));
        return path;
    }

    private static DocumentRecord Document(string path)
    {
        var info = new FileInfo(path);
        return DocumentRecord.Restore(
            DocumentId.New(),
            SourceId.New(),
            new Uri(path, UriKind.Absolute).AbsoluteUri,
            info.Name,
            info.Length,
            info.CreationTimeUtc,
            info.LastWriteTimeUtc,
            DateTimeOffset.UtcNow,
            ScanRunId.New(),
            missingSinceUtc: null);
    }

    public void Dispose()
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
