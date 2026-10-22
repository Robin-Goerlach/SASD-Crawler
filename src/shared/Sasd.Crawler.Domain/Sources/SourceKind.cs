namespace Sasd.Crawler.Domain.Sources;

/// <summary>
/// Describes the logical kind of a crawl source.
/// </summary>
/// <remarks>
/// The enum intentionally models product concepts rather than implementation classes. A source kind therefore
/// remains stable even if its connector implementation is replaced later.
/// </remarks>
public enum SourceKind
{
    /// <summary>A local directory or local fixed drive visible to the current Windows user.</summary>
    LocalDirectory = 1,

    /// <summary>A removable/offline-capable medium such as a USB disk.</summary>
    RemovableMedia = 2,

    /// <summary>A Windows SMB/UNC network location.</summary>
    NetworkShare = 3,

    /// <summary>An HTTP/HTTPS website crawl root.</summary>
    Website = 4,
}
