using System.Globalization;
using Microsoft.Data.Sqlite;
using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Persistence.Sqlite.Sources;

/// <summary>
/// Stores source definitions in the per-user SQLite control database.
/// </summary>
/// <remarks>
/// SQLite is the durable lifecycle/control truth for the desktop product. Lucene remains a rebuildable derived
/// index and therefore must never become the only owner of source identity or enabled state.
/// </remarks>
public sealed class SqliteSourceRepository : ISourceRepository
{
    private const int CurrentSchemaVersion = 1;
    private readonly string connectionString;

    /// <summary>
    /// Initializes a repository for the supplied control database path.
    /// </summary>
    /// <param name="databasePath">Path of the SQLite control database file.</param>
    public SqliteSourceRepository(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("A control database path is required.", nameof(databasePath));
        }

        DatabasePath = Path.GetFullPath(databasePath);

        // Pooling is deliberately disabled for the first production slice. This makes file ownership and test
        // cleanup deterministic. Connection pooling can be measured and enabled later during the optimization
        // round rather than obscuring lifecycle behavior now.
        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
        }.ToString();
    }

    /// <summary>Gets the absolute control-database path used by this repository.</summary>
    public string DatabasePath { get; }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(DatabasePath)
            ?? throw new InvalidOperationException("The control database path has no parent directory.");

        Directory.CreateDirectory(directory);

        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // WAL provides good resilience and reader/writer coexistence for the single-user desktop model. It is
        // established before migrations so every subsequent connection sees the intended journal mode.
        await ExecuteNonQueryAsync(connection, "PRAGMA journal_mode = WAL;", cancellationToken)
            .ConfigureAwait(false);

        var version = await ReadUserVersionAsync(connection, cancellationToken).ConfigureAwait(false);
        switch (version)
        {
            case 0:
                await ApplyVersion1Async(connection, cancellationToken).ConfigureAwait(false);
                break;
            case CurrentSchemaVersion:
                break;
            default:
                // Opening a newer database with older code could corrupt or silently misread state. Failing
                // closed is safer than attempting an implicit downgrade.
                throw new NotSupportedException(
                    $"Control database schema version {version} is newer than supported version {CurrentSchemaVersion}.");
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SourceDefinition>> ListAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                source_id,
                kind,
                display_name,
                canonical_location,
                is_enabled,
                created_utc,
                updated_utc
            FROM sources
            ORDER BY display_name COLLATE NOCASE, canonical_location COLLATE NOCASE;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var result = new List<SourceDefinition>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(ReadSource(reader));
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<SourceDefinition?> FindByCanonicalLocationAsync(
        string canonicalLocation,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalLocation);

        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                source_id,
                kind,
                display_name,
                canonical_location,
                is_enabled,
                created_utc,
                updated_utc
            FROM sources
            WHERE canonical_location = $canonicalLocation COLLATE NOCASE
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$canonicalLocation", canonicalLocation);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false)
            ? ReadSource(reader)
            : null;
    }

    /// <inheritdoc />
    public async Task<bool> TryAddAsync(SourceDefinition source, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);

        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT OR IGNORE INTO sources (
                source_id,
                kind,
                display_name,
                canonical_location,
                is_enabled,
                created_utc,
                updated_utc)
            VALUES (
                $sourceId,
                $kind,
                $displayName,
                $canonicalLocation,
                $isEnabled,
                $createdUtc,
                $updatedUtc);
            """;
        BindSource(command, source);

        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) == 1;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(SourceDefinition source, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);

        await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE sources
            SET
                display_name = $displayName,
                is_enabled = $isEnabled,
                updated_utc = $updatedUtc
            WHERE source_id = $sourceId;
            """;
        command.Parameters.AddWithValue("$displayName", source.DisplayName);
        command.Parameters.AddWithValue("$isEnabled", source.IsEnabled ? 1 : 0);
        command.Parameters.AddWithValue(
            "$updatedUtc",
            source.UpdatedAtUtc.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$sourceId", source.Id.ToString());

        var affected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (affected != 1)
        {
            throw new KeyNotFoundException($"Source '{source.Id}' no longer exists in the control store.");
        }
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            // Busy timeout prevents a transient writer overlap from failing immediately while still providing a
            // bounded wait. Longer retry/backoff policy belongs to the later durable-work-queue milestone.
            await ExecuteNonQueryAsync(connection, "PRAGMA busy_timeout = 5000;", cancellationToken)
                .ConfigureAwait(false);
            await ExecuteNonQueryAsync(connection, "PRAGMA foreign_keys = ON;", cancellationToken)
                .ConfigureAwait(false);

            return connection;
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private static async Task<int> ReadUserVersionAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version;";
        var value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static async Task ApplyVersion1Async(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
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
                """;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using (var versionCommand = connection.CreateCommand())
        {
            versionCommand.Transaction = transaction;
            versionCommand.CommandText = $"PRAGMA user_version = {CurrentSchemaVersion};";
            await versionCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void BindSource(SqliteCommand command, SourceDefinition source)
    {
        command.Parameters.AddWithValue("$sourceId", source.Id.ToString());
        command.Parameters.AddWithValue("$kind", source.Kind.ToString());
        command.Parameters.AddWithValue("$displayName", source.DisplayName);
        command.Parameters.AddWithValue("$canonicalLocation", source.CanonicalLocation);
        command.Parameters.AddWithValue("$isEnabled", source.IsEnabled ? 1 : 0);
        command.Parameters.AddWithValue(
            "$createdUtc",
            source.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue(
            "$updatedUtc",
            source.UpdatedAtUtc.ToString("O", CultureInfo.InvariantCulture));
    }

    private static SourceDefinition ReadSource(SqliteDataReader reader)
    {
        var kindText = reader.GetString(1);
        if (!Enum.TryParse<SourceKind>(kindText, ignoreCase: false, out var kind))
        {
            throw new InvalidDataException($"Unknown persisted source kind '{kindText}'.");
        }

        return SourceDefinition.Restore(
            SourceId.Parse(reader.GetString(0)),
            kind,
            reader.GetString(2),
            reader.GetString(3),
            reader.GetInt32(4) != 0,
            ParseTimestamp(reader.GetString(5)),
            ParseTimestamp(reader.GetString(6)));
    }

    private static DateTimeOffset ParseTimestamp(string value)
        => DateTimeOffset.Parse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);

    private static async Task ExecuteNonQueryAsync(
        SqliteConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
