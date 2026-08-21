using Microsoft.Data.Sqlite;
using Sasd.Crawler.Spike.A1.Core;

namespace Sasd.Crawler.Spike.A1.Infrastructure;

/// <summary>SQLite implementation using short-lived connections so ordered shutdown releases the file.</summary>
public sealed class SqliteHeartbeatStore(string databasePath) : IHeartbeatStore
{
    public string DatabasePath { get; } = databasePath;
    // Pooling is disabled for the spike so graceful host shutdown has a directly verifiable file-handle boundary.
    private string ConnectionString => new SqliteConnectionStringBuilder { DataSource = DatabasePath, Pooling = false }.ToString();

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        await using var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await ExecuteAsync(connection, "PRAGMA journal_mode=WAL;", cancellationToken).ConfigureAwait(false);
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS heartbeat (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                timestamp_utc TEXT NOT NULL,
                status TEXT NOT NULL,
                process_id INTEGER NOT NULL
            );
            """, cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteAsync(Heartbeat heartbeat, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO heartbeat (id, timestamp_utc, status, process_id)
            VALUES (1, $timestamp, $status, $processId)
            ON CONFLICT(id) DO UPDATE SET timestamp_utc=$timestamp, status=$status, process_id=$processId;
            """;
        command.Parameters.AddWithValue("$timestamp", heartbeat.TimestampUtc.ToString("O"));
        command.Parameters.AddWithValue("$status", heartbeat.Status);
        command.Parameters.AddWithValue("$processId", heartbeat.ProcessId);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Heartbeat?> ReadLatestAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT timestamp_utc, status, process_id FROM heartbeat WHERE id = 1;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) return null;
        return new Heartbeat(DateTimeOffset.Parse(reader.GetString(0)), reader.GetString(1), reader.GetInt32(2));
    }

    private static async Task ExecuteAsync(SqliteConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
