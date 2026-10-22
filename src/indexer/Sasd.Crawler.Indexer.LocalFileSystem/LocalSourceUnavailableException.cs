namespace Sasd.Crawler.Indexer.LocalFileSystem;

/// <summary>
/// Indicates that the configured local source root cannot be used as the start of a scan.
/// </summary>
public sealed class LocalSourceUnavailableException : IOException
{
    /// <summary>Creates an exception for an unavailable source root.</summary>
    public LocalSourceUnavailableException(string rootPath)
        : base($"The configured local source root '{rootPath}' is not available.")
    {
        RootPath = rootPath;
    }

    /// <summary>Gets the normalized local root path that could not be scanned.</summary>
    public string RootPath { get; }
}
