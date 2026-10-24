using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Sasd.Crawler.Application.Extraction;
using Sasd.Crawler.Domain.Documents;

namespace Sasd.Crawler.Extraction.Native;

/// <summary>
/// In-process, read-only extractor for plain-text and HTML files used by milestone 0.1.
/// </summary>
/// <remarks>
/// HTML is parsed into a DOM with AngleSharp. No browsing context, resource loader or JavaScript engine is created,
/// therefore referenced scripts, stylesheets and remote resources are not fetched or executed. Rich document
/// formats remain outside this class and will use the isolated Tika sidecar behind <see cref="IContentExtractor"/>.
/// </remarks>
public sealed class NativeTextHtmlContentExtractor : IContentExtractor
{
    private static readonly HashSet<string> SupportedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".txt", ".html", ".htm" };

    private readonly NativeContentExtractionOptions options;

    /// <summary>Creates the extractor with resource-limit options.</summary>
    public NativeTextHtmlContentExtractor(NativeContentExtractionOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        options.Validate();
    }

    /// <inheritdoc />
    public bool CanExtract(DocumentRecord document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return TryResolveLocalPath(document.CanonicalLocator, out var path)
            && SupportedExtensions.Contains(Path.GetExtension(path));
    }

    /// <inheritdoc />
    public async Task<ExtractedContent> ExtractAsync(
        DocumentRecord document,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!TryResolveLocalPath(document.CanonicalLocator, out var path)
            || !SupportedExtensions.Contains(Path.GetExtension(path)))
        {
            throw new NotSupportedException(
                $"Native TXT/HTML extraction does not support '{document.CanonicalLocator}'.");
        }

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 64 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        if (stream.Length > options.MaximumInputBytes)
        {
            throw new ContentExtractionLimitExceededException(
                path,
                stream.Length,
                options.MaximumInputBytes);
        }

        var extension = Path.GetExtension(path);
        return extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)
            ? await ExtractTextAsync(stream, cancellationToken).ConfigureAwait(false)
            : await ExtractHtmlAsync(stream, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<ExtractedContent> ExtractTextAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        // StreamReader detects UTF BOMs and otherwise uses UTF-8. Legacy code-page detection is intentionally
        // deferred; silently guessing an encoding would be worse than a clearly versioned future enhancement.
        using var reader = new StreamReader(
            stream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 64 * 1024,
            leaveOpen: true);

        var text = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        return new ExtractedContent(
            text,
            Title: null,
            MediaType: "text/plain",
            Language: "und",
            ExtractorName: "SASD Native Text",
            ExtractorVersion: "1");
    }

    private static async Task<ExtractedContent> ExtractHtmlAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(
            stream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 64 * 1024,
            leaveOpen: true);

        var html = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(html, cancellationToken).ConfigureAwait(false);

        // Script/style/template text is not user-visible page content and would create noisy false-positive hits.
        // Removing these nodes also ensures potentially hostile script source is treated as inert data only.
        foreach (var element in document.QuerySelectorAll("script, style, noscript, template").ToArray())
        {
            element.Remove();
        }

        var bodyText = document.Body?.TextContent
            ?? document.DocumentElement?.TextContent
            ?? string.Empty;

        var language = NormalizeLanguage(document.DocumentElement?.GetAttribute("lang"));

        return new ExtractedContent(
            NormalizeWhitespace(bodyText),
            string.IsNullOrWhiteSpace(document.Title) ? null : document.Title.Trim(),
            MediaType: "text/html",
            Language: language,
            ExtractorName: "SASD Native HTML/AngleSharp",
            ExtractorVersion: "1");
    }

    private static bool TryResolveLocalPath(string canonicalLocator, out string path)
    {
        if (Uri.TryCreate(canonicalLocator, UriKind.Absolute, out var uri) && uri.IsFile)
        {
            path = Path.GetFullPath(uri.LocalPath);
            return true;
        }

        path = string.Empty;
        return false;
    }

    private static string NormalizeLanguage(string? language)
        => string.IsNullOrWhiteSpace(language)
            ? "und"
            : language.Trim().ToLowerInvariant();

    private static string NormalizeWhitespace(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        var previousWasWhitespace = false;

        foreach (var character in value)
        {
            if (char.IsWhiteSpace(character))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(' ');
                    previousWasWhitespace = true;
                }
            }
            else
            {
                builder.Append(character);
                previousWasWhitespace = false;
            }
        }

        return builder.ToString().Trim();
    }
}
