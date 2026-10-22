using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Persistence.Sqlite.Sources;

namespace Sasd.Crawler.Persistence.Sqlite.Tests.Sources;

/// <summary>
/// Exercises the first production SQLite migration and source persistence against a real temporary database.
/// </summary>
public sealed class SqliteSourceRepositoryTests
{
    [Fact]
    public async Task Src001_InitializeAndInsert_PersistsSourceAcrossRepositoryInstances()
    {
        var fixture = new TemporaryDatabase();
        try
        {
            var created = new DateTimeOffset(2026, 10, 22, 19, 0, 0, TimeSpan.Zero);
            var source = SourceDefinition.CreateLocalDirectory(
                "Documents",
                "file:///C:/Data/Documents/",
                created);

            var writer = new SqliteSourceRepository(fixture.DatabasePath);
            await writer.InitializeAsync(CancellationToken.None);
            Assert.True(await writer.TryAddAsync(source, CancellationToken.None));

            // A fresh repository instance proves that the result came from durable SQLite state rather than
            // accidental process memory.
            var reader = new SqliteSourceRepository(fixture.DatabasePath);
            await reader.InitializeAsync(CancellationToken.None);
            var persisted = Assert.Single(await reader.ListAsync(CancellationToken.None));

            Assert.Equal(source.Id, persisted.Id);
            Assert.Equal(source.CanonicalLocation, persisted.CanonicalLocation);
            Assert.True(persisted.IsEnabled);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Src001_CanonicalLocation_IsUniqueIgnoringWindowsStyleCase()
    {
        var fixture = new TemporaryDatabase();
        try
        {
            var now = new DateTimeOffset(2026, 10, 22, 19, 0, 0, TimeSpan.Zero);
            var repository = new SqliteSourceRepository(fixture.DatabasePath);
            await repository.InitializeAsync(CancellationToken.None);

            var first = SourceDefinition.CreateLocalDirectory(
                "Documents",
                "file:///C:/Data/Documents/",
                now);
            var sameLocationDifferentCase = SourceDefinition.CreateLocalDirectory(
                "Duplicate",
                "file:///c:/data/documents/",
                now);

            Assert.True(await repository.TryAddAsync(first, CancellationToken.None));
            Assert.False(await repository.TryAddAsync(sameLocationDifferentCase, CancellationToken.None));
            Assert.Single(await repository.ListAsync(CancellationToken.None));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task SchemaInitialization_IsIdempotent()
    {
        var fixture = new TemporaryDatabase();
        try
        {
            var repository = new SqliteSourceRepository(fixture.DatabasePath);

            await repository.InitializeAsync(CancellationToken.None);
            await repository.InitializeAsync(CancellationToken.None);

            Assert.Empty(await repository.ListAsync(CancellationToken.None));
        }
        finally
        {
            fixture.Dispose();
        }
    }

    private sealed class TemporaryDatabase : IDisposable
    {
        private readonly string directory = Path.Combine(
            Path.GetTempPath(),
            "SasdCrawlerTests",
            Guid.NewGuid().ToString("N"));

        public TemporaryDatabase()
        {
            Directory.CreateDirectory(directory);
            DatabasePath = Path.Combine(directory, "control.db");
        }

        public string DatabasePath { get; }

        public void Dispose()
        {
            // Pooling is disabled by SqliteSourceRepository, so all short-lived connections release the database
            // file deterministically and the test can clean up without process-wide pool management.
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
