using Microsoft.Data.Sqlite;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Persistence.Sqlite.Sources;

namespace Sasd.Crawler.Persistence.Sqlite.Tests;

/// <summary>
/// Verifies forward-only migration of the existing production schema instead of recreating user data.
/// </summary>
public sealed class SqliteControlStoreMigrationTests
{
    [Fact]
    public async Task ExistingVersion1Database_MigratesToVersion2WithoutLosingSource()
    {
        using var fixture = new LegacyVersion1Database();
        var sourceId = SourceId.New();
        fixture.Create(sourceId);

        var controlStore = new SqliteControlStore(fixture.DatabasePath);
        await controlStore.InitializeAsync(CancellationToken.None);

        var repository = new SqliteSourceRepository(controlStore);
        var source = await repository.GetByIdAsync(sourceId, CancellationToken.None);

        Assert.NotNull(source);
        Assert.Equal("Legacy", source.DisplayName);
        Assert.Equal(2, fixture.ReadUserVersion());
    }

    private sealed class LegacyVersion1Database : IDisposable
    {
        private readonly string directory = Path.Combine(
            Path.GetTempPath(),
            "SasdCrawlerMigrationTests",
            Guid.NewGuid().ToString("N"));

        public LegacyVersion1Database()
        {
            Directory.CreateDirectory(directory);
            DatabasePath = Path.Combine(directory, "control.db");
        }

        public string DatabasePath { get; }

        public void Create(SourceId sourceId)
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath};Pooling=False");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE sources (
                    source_id TEXT PRIMARY KEY,
                    kind TEXT NOT NULL,
                    display_name TEXT NOT NULL,
                    canonical_location TEXT NOT NULL COLLATE NOCASE,
                    is_enabled INTEGER NOT NULL CHECK (is_enabled IN (0, 1)),
                    created_utc TEXT NOT NULL,
                    updated_utc TEXT NOT NULL
                );

                CREATE UNIQUE INDEX ux_sources_canonical_location
                    ON sources(canonical_location COLLATE NOCASE);

                INSERT INTO sources (
                    source_id,
                    kind,
                    display_name,
                    canonical_location,
                    is_enabled,
                    created_utc,
                    updated_utc)
                VALUES (
                    $sourceId,
                    'LocalDirectory',
                    'Legacy',
                    'file:///C:/Legacy/',
                    1,
                    '2026-10-22T17:00:00.0000000+00:00',
                    '2026-10-22T17:00:00.0000000+00:00');

                PRAGMA user_version = 1;
                """;
            command.Parameters.AddWithValue("$sourceId", sourceId.ToString());
            command.ExecuteNonQuery();
        }

        public int ReadUserVersion()
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath};Pooling=False");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA user_version;";
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public void Dispose()
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
