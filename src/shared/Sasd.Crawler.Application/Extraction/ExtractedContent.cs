namespace Sasd.Crawler.Application.Extraction;

/// <summary>
/// Safe, normalized text and metadata produced by one content extractor.
/// </summary>
/// <remarks>
/// Extracted content is derived state. The original file remains authoritative and is never modified by the
/// extraction pipeline.
/// </remarks>
public sealed record ExtractedContent(
    string Text,
    string? Title,
    string MediaType,
    string Language,
    string ExtractorName,
    string ExtractorVersion);
