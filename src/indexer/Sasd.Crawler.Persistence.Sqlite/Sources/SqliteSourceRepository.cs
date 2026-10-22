using System.Globalization;
using Microsoft.Data.Sqlite;
using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Persistence.Sqlite;

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
    private readonly SqliteControlStore controlStore;

    /// <summary>Creates a source repository for a database path.</summary>
    public SqliteSourceRepository(string databasePath)
        : this(new SqliteControlStore(databasePath))
    {
    }

    /// <summary>Creates a source repository over the shared control-store boundary.</summary>
    public SqliteSourceRepository(SqliteControlStore controlStore)
    {
        this.controlStore = controlStore ?? throw new ArgumentNullException(nameof(controlStore));
    }

    /// <summary>Gets the absolute control-database path used by this repository.</summary>
    public string DatabasePath => controlStore.DatabasePath;

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken)
        => controlStore.InitializeAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<SourceDefinition>> ListAsync(CancellationToken cancellationToken)
    {
        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
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
    public async Task<SourceDefinition?> GetByIdAsync(
        SourceId sourceId,
        CancellationToken cancellationToken)
    {
        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
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
            WHERE source_id = $sourceId
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$sourceId", sourceId.ToString());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false)
            ? ReadSource(reader)
            : null;
    }

    /// <inheritdoc />
    public async Task<SourceDefinition?> FindByCanonicalLocationAsync(
        string canonicalLocation,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalLocation);

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
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

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
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

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
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
}
