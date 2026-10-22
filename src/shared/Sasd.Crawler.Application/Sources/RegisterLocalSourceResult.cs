namespace Sasd.Crawler.Application.Sources;

/// <summary>
/// Describes the result of registering a local directory.
/// </summary>
/// <param name="Source">The newly created or already existing source.</param>
/// <param name="Created">
/// <see langword="true"/> when a new source was persisted; <see langword="false"/> when the same canonical
/// location was already registered.
/// </param>
public sealed record RegisterLocalSourceResult(SourceSummary Source, bool Created);
