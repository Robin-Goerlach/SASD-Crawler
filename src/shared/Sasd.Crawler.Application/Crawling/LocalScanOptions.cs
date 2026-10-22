namespace Sasd.Crawler.Application.Crawling;

/// <summary>
/// Controls the scope of a local full scan.
/// </summary>
public sealed record LocalScanOptions
{
    /// <summary>Creates scan options.</summary>
    /// <param name="maximumDepth">
    /// Maximum directory depth below the configured root. Zero means root files only; <see langword="null"/> means
    /// no configured depth limit.
    /// </param>
    public LocalScanOptions(int? maximumDepth = null)
    {
        if (maximumDepth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumDepth), "Maximum depth must be zero or greater.");
        }

        MaximumDepth = maximumDepth;
    }

    /// <summary>Gets the maximum directory depth below the source root.</summary>
    public int? MaximumDepth { get; }
}
