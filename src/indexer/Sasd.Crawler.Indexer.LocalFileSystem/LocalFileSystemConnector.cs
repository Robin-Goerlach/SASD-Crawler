using System.Diagnostics.CodeAnalysis;
using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Indexer.LocalFileSystem;

/// <summary>
/// Streams files from a local-directory source while preserving reconciliation safety.
/// </summary>
/// <remarks>
/// The implementation intentionally does not use <c>SearchOption.AllDirectories</c>. Explicit directory traversal
/// lets us enforce depth limits, avoid following reparse points and turn recoverable per-directory/per-file errors
/// into scan issues instead of silently skipping unknown subtrees.
/// </remarks>
public sealed class LocalFileSystemConnector : ILocalFileSystemConnector
{
    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        RecurseSubdirectories = false,
        IgnoreInaccessible = false,
        ReturnSpecialDirectories = false,
        AttributesToSkip = 0,
    };

    /// <inheritdoc />
    public IEnumerable<LocalScanEvent> Enumerate(
        SourceDefinition source,
        LocalScanOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(options);

        if (source.Kind != SourceKind.LocalDirectory)
        {
            throw new ArgumentException(
                $"Source '{source.Id}' is '{source.Kind}', not a local directory.",
                nameof(source));
        }

        var rootPath = ResolveRootPath(source);
        if (!Directory.Exists(rootPath))
        {
            // Directory.Exists deliberately maps many access/not-found conditions to false. At the root there is
            // no meaningful partial scan: without a root we cannot prove anything about source completeness.
            throw new LocalSourceUnavailableException(rootPath);
        }

        var pendingDirectories = new Stack<DirectoryWorkItem>();
        pendingDirectories.Push(new DirectoryWorkItem(rootPath, Depth: 0));

        while (pendingDirectories.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var current = pendingDirectories.Pop();
            if (!TryCreateEnumerator(current.Path, out var enumerator, out var openError))
            {
                // A child directory that cannot be enumerated does not prevent us from scanning siblings. The
                // emitted issue makes the overall run Partial, so missing reconciliation stays disabled.
                yield return new LocalScanIssueReported(current.Path, openError!);
                continue;
            }

            using (enumerator)
            {
                string? enumerationError = null;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!TryMoveNext(enumerator, out var entryPath, out var moveError))
                    {
                        if (moveError is not null)
                        {
                            enumerationError = moveError;
                        }

                        break;
                    }

                    if (!TryReadAttributes(entryPath!, out var attributes, out var attributeError))
                    {
                        yield return new LocalScanIssueReported(entryPath!, attributeError!);
                        continue;
                    }

                    // Reparse points are intentionally excluded from the authoritative scan scope for now. This
                    // avoids junction/symlink loops until the later LOC-004 policy explicitly supports following
                    // them with identity/loop detection.
                    if ((attributes & FileAttributes.ReparsePoint) != 0)
                    {
                        continue;
                    }

                    if ((attributes & FileAttributes.Directory) != 0)
                    {
                        if (!options.MaximumDepth.HasValue || current.Depth < options.MaximumDepth.Value)
                        {
                            pendingDirectories.Push(new DirectoryWorkItem(entryPath!, current.Depth + 1));
                        }

                        continue;
                    }

                    if (!TryCreateObservation(entryPath!, out var observation, out var metadataError))
                    {
                        // The path itself was visible but metadata could not be read. Continuing satisfies LOC-005;
                        // the issue still prevents the run from becoming authoritative.
                        yield return new LocalScanIssueReported(entryPath!, metadataError!);
                        continue;
                    }

                    yield return new LocalFileObserved(observation!);
                }

                if (enumerationError is not null)
                {
                    // Enumeration may fail after yielding some entries (for example because permissions change).
                    // Preserve those discoveries, flag the run partial, and continue with already discovered
                    // sibling directories rather than discarding useful progress.
                    yield return new LocalScanIssueReported(current.Path, enumerationError);
                }
            }
        }
    }

    private static string ResolveRootPath(SourceDefinition source)
    {
        if (!Uri.TryCreate(source.CanonicalLocation, UriKind.Absolute, out var uri) || !uri.IsFile)
        {
            throw new InvalidDataException(
                $"Local source '{source.Id}' has invalid file URI '{source.CanonicalLocation}'.");
        }

        return Path.GetFullPath(uri.LocalPath);
    }

    private static bool TryCreateEnumerator(
        string directoryPath,
        [NotNullWhen(true)] out IEnumerator<string>? enumerator,
        out string? error)
    {
        try
        {
            enumerator = Directory
                .EnumerateFileSystemEntries(directoryPath, "*", EnumerationOptions)
                .GetEnumerator();
            error = null;
            return true;
        }
        catch (Exception exception) when (IsRecoverableFileSystemException(exception))
        {
            enumerator = null;
            error = BuildIssueMessage(exception);
            return false;
        }
    }

    private static bool TryMoveNext(
        IEnumerator<string> enumerator,
        out string? entryPath,
        out string? error)
    {
        try
        {
            if (!enumerator.MoveNext())
            {
                entryPath = null;
                error = null;
                return false;
            }

            entryPath = enumerator.Current;
            error = null;
            return true;
        }
        catch (Exception exception) when (IsRecoverableFileSystemException(exception))
        {
            entryPath = null;
            error = BuildIssueMessage(exception);
            return false;
        }
    }

    private static bool TryReadAttributes(
        string path,
        out FileAttributes attributes,
        out string? error)
    {
        try
        {
            attributes = File.GetAttributes(path);
            error = null;
            return true;
        }
        catch (Exception exception) when (IsRecoverableFileSystemException(exception))
        {
            attributes = default;
            error = BuildIssueMessage(exception);
            return false;
        }
    }

    private static bool TryCreateObservation(
        string path,
        out FileObservation? observation,
        out string? error)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            var fileInfo = new FileInfo(fullPath);

            observation = new FileObservation(
                new Uri(fullPath, UriKind.Absolute).AbsoluteUri,
                fileInfo.Name,
                fileInfo.Length,
                new DateTimeOffset(fileInfo.CreationTimeUtc),
                new DateTimeOffset(fileInfo.LastWriteTimeUtc));
            error = null;
            return true;
        }
        catch (Exception exception) when (IsRecoverableFileSystemException(exception))
        {
            observation = null;
            error = BuildIssueMessage(exception);
            return false;
        }
    }

    private static bool IsRecoverableFileSystemException(Exception exception)
        => exception is IOException or UnauthorizedAccessException;

    private static string BuildIssueMessage(Exception exception)
        => $"{exception.GetType().Name}: {exception.Message}";

    private sealed record DirectoryWorkItem(string Path, int Depth);
}
