namespace Sasd.Crawler.Application.Crawling;

/// <summary>
/// Contains filesystem metadata captured when a file location is successfully observed.
/// </summary>
public sealed record FileObservation
{
    /// <summary>Creates a file observation.</summary>
    public FileObservation(
        string canonicalLocator,
        string displayName,
        long fileSize,
        DateTimeOffset? createdAtUtc,
        DateTimeOffset modifiedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(canonicalLocator))
        {
            throw new ArgumentException("A canonical locator is required.", nameof(canonicalLocator));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("A display name is required.", nameof(displayName));
        }

        if (fileSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fileSize), "File size must not be negative.");
        }

        CanonicalLocator = canonicalLocator.Trim();
        DisplayName = displayName.Trim();
        FileSize = fileSize;
        CreatedAtUtc = createdAtUtc;
        ModifiedAtUtc = modifiedAtUtc;
    }

    /// <summary>Gets the canonical absolute file URI.</summary>
    public string CanonicalLocator { get; }

    /// <summary>Gets the filesystem file name.</summary>
    public string DisplayName { get; }

    /// <summary>Gets the observed file size in bytes.</summary>
    public long FileSize { get; }

    /// <summary>Gets the observed creation timestamp when available.</summary>
    public DateTimeOffset? CreatedAtUtc { get; }

    /// <summary>Gets the observed last-write timestamp.</summary>
    public DateTimeOffset ModifiedAtUtc { get; }
}
