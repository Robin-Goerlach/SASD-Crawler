namespace Sasd.Crawler.Extraction.Native;

/// <summary>
/// Resource limits for the in-process TXT/HTML extractor.
/// </summary>
public sealed record NativeContentExtractionOptions
{
    /// <summary>
    /// Gets the maximum original file size accepted by the native extractor.
    /// </summary>
    /// <remarks>
    /// The default is deliberately modest for the first desktop slice. It is configurable so later benchmarking
    /// can raise it without changing extraction semantics.
    /// </remarks>
    public long MaximumInputBytes { get; init; } = 16L * 1024L * 1024L;

    internal void Validate()
    {
        if (MaximumInputBytes <= 0)
        {
            throw new InvalidOperationException("Native extraction maximum input size must be positive.");
        }
    }
}
