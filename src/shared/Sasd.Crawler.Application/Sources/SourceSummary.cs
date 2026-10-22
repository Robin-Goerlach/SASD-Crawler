using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Sources;

/// <summary>
/// Read-only application model used by presentation layers when displaying configured sources.
/// </summary>
public sealed record SourceSummary(
    SourceId Id,
    SourceKind Kind,
    string DisplayName,
    string CanonicalLocation,
    string DisplayLocation,
    bool IsEnabled);
