using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Indexer.LocalFileSystem;

namespace Sasd.Crawler.Indexer.LocalFileSystem.Tests;

/// <summary>
/// Exercises real filesystem traversal against disposable test directories.
/// </summary>
public sealed class LocalFileSystemConnectorTests
{
    [Fact]
    public void Loc002_Enumerate_RecursesAndReturnsFileMetadata()
    {
        using var fixture = new TemporaryTree();
        fixture.Write("root.txt", "root");
        fixture.Write(Path.Combine("child", "nested.txt"), "nested");

        var events = EnumerateFiles(fixture, new LocalScanOptions()).ToArray();

        Assert.Equal(2, events.Length);
        Assert.Contains(events, file => file.Observation.DisplayName == "root.txt");
        Assert.Contains(events, file => file.Observation.DisplayName == "nested.txt");
        Assert.All(events, file => Assert.True(file.Observation.FileSize > 0));
        Assert.All(events, file => Assert.StartsWith("file:", file.Observation.CanonicalLocator, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Loc003_MaximumDepth_ExcludesDeeperDirectories()
    {
        using var fixture = new TemporaryTree();
        fixture.Write("root.txt", "root");
        fixture.Write(Path.Combine("child", "level1.txt"), "level1");
        fixture.Write(Path.Combine("child", "grandchild", "level2.txt"), "level2");

        var files = EnumerateFiles(fixture, new LocalScanOptions(maximumDepth: 1))
            .Select(file => file.Observation.DisplayName)
            .ToArray();

        Assert.Contains("root.txt", files);
        Assert.Contains("level1.txt", files);
        Assert.DoesNotContain("level2.txt", files);
    }

    [Fact]
    public void Loc001_MissingRoot_FailsWithoutPretendingScanIsComplete()
    {
        var missing = Path.Combine(Path.GetTempPath(), "SasdCrawlerMissing", Guid.NewGuid().ToString("N"));
        var source = SourceDefinition.CreateLocalDirectory(
            "Missing",
            new Uri(missing + Path.DirectorySeparatorChar, UriKind.Absolute).AbsoluteUri,
            DateTimeOffset.UtcNow);

        var connector = new LocalFileSystemConnector();

        Assert.Throws<LocalSourceUnavailableException>(
            () => connector.Enumerate(source, new LocalScanOptions(), CancellationToken.None).ToArray());
    }

    [Fact]
    public void Cancellation_StopsStreamingTraversal()
    {
        using var fixture = new TemporaryTree();
        fixture.Write("a.txt", "a");

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var connector = new LocalFileSystemConnector();

        Assert.Throws<OperationCanceledException>(
            () => connector
                .Enumerate(fixture.Source, new LocalScanOptions(), cancellation.Token)
                .ToArray());
    }

    private static IEnumerable<LocalFileObserved> EnumerateFiles(
        TemporaryTree fixture,
        LocalScanOptions options)
        => new LocalFileSystemConnector()
            .Enumerate(fixture.Source, options, CancellationToken.None)
            .OfType<LocalFileObserved>();

    private sealed class TemporaryTree : IDisposable
    {
        public TemporaryTree()
        {
            RootPath = Path.Combine(
                Path.GetTempPath(),
                "SasdCrawlerLocalConnectorTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(RootPath);

            Source = SourceDefinition.CreateLocalDirectory(
                "Test tree",
                new Uri(RootPath + Path.DirectorySeparatorChar, UriKind.Absolute).AbsoluteUri,
                DateTimeOffset.UtcNow);
        }

        public string RootPath { get; }

        public SourceDefinition Source { get; }

        public void Write(string relativePath, string content)
        {
            var path = Path.Combine(RootPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }

        public void Dispose()
        {
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, recursive: true);
            }
        }
    }
}
