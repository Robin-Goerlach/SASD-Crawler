using Sasd.Crawler.Domain.Documents;

namespace Sasd.Crawler.Application.Extraction;

/// <summary>
/// Extracts searchable text from one durable document record.
/// </summary>
/// <remarks>
/// Implementations must treat source files as untrusted input, open originals read-only and honor cancellation and
/// resource limits. External parser implementations may live behind this same boundary later.
/// </remarks>
public interface IContentExtractor
{
    /// <summary>Returns whether this extractor is responsible for the supplied document.</summary>
    bool CanExtract(DocumentRecord document);

    /// <summary>Extracts normalized searchable content without modifying the original document.</summary>
    Task<ExtractedContent> ExtractAsync(
        DocumentRecord document,
        CancellationToken cancellationToken);
}
