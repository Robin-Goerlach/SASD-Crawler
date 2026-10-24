namespace Sasd.Crawler.Extraction.Native;

/// <summary>
/// Indicates that a source document exceeds the configured native extraction size limit.
/// </summary>
public sealed class ContentExtractionLimitExceededException : IOException
{
    /// <summary>Creates the limit failure.</summary>
    public ContentExtractionLimitExceededException(string path, long length, long limit)
        : base($"File '{path}' is {length} bytes and exceeds the extraction limit of {limit} bytes.")
    {
        Path = path;
        Length = length;
        Limit = limit;
    }

    /// <summary>Gets the affected local path.</summary>
    public string Path { get; }

    /// <summary>Gets the observed input length.</summary>
    public long Length { get; }

    /// <summary>Gets the configured maximum length.</summary>
    public long Limit { get; }
}
