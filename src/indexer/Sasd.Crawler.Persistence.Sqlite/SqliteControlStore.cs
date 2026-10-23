using System.Globalization;
using Microsoft.Data.Sqlite;

namespace Sasd.Crawler.Persistence.Sqlite;

/// <summary>
/// Owns the SQLite control-store connection policy and forward-only schema migrations.
/// </summary>
/// <remarks>
/// All SQLite-backed repositories share this migration boundary. Keeping schema ownership centralized prevents
/// each repository from independently guessing which tables or schema version should exist.
/// </remarks>
public sealed class SqliteControlStore
{
    private const int CurrentSchemaVersion = 3;
    private readonly string connectionString;
    private readonly SemaphoreSlim initializationGate = new(1, 1);
    private bool initialized;

    /// <summary>Creates a control store for the supplied SQLite database path.</summary>
    public SqliteControlStore(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("A control database path is required.", nameof(databasePath));
        }

        DatabasePath = Path.GetFullPath(databasePath);

        // Pooling stays disabled during the correctness-first phase. Deterministic handle release makes migration,
        // crash/recovery and disposable integration tests easier to reason about. It can be measured later.
        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
        }.ToString();
    }

    /// <summary>Gets the absolute SQLite control-store path.</summary>
    public string DatabasePath { get; }

    /// <summary>Creates the database when needed and migrates it to the supported schema version.</summary>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (initialized)
        {
            return;
        }

        // Multiple repositories are started by the Generic Host. Serializing in-process initialization prevents
        // those startup paths from racing the same migration chain. Cross-process startup coordination is not
        // claimed here; the production single-instance boundary remains a separate lifecycle slice.
        await initializationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (initialized)
            {
                return;
            }

            var directory = Path.GetDirectoryName(DatabasePath)
                ?? throw new InvalidOperationException("The control database path has no parent directory.");

            Directory.CreateDirectory(directory);

            await using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

            // WAL is a database-level setting and is established before migrations so all repositories observe the
            // same writer/read behavior.
            await ExecuteNonQueryAsync(connection, "PRAGMA journal_mode = WAL;", cancellationToken)
                .ConfigureAwait(false);

            var version = await ReadUserVersionAsync(connection, cancellationToken).ConfigureAwait(false);
            if (version > CurrentSchemaVersion)
            {
                // An older executable must never attempt an implicit downgrade of a newer control database.
                throw new NotSupportedException(
                    $"Control database schema version {version} is newer than supported version {CurrentSchemaVersion}.");
            }

            while (version < CurrentSchemaVersion)
            {
                version = version switch
                {
                    0 => await ApplyVersion1Async(connection, cancellationToken).ConfigureAwait(false),
                    1 => await ApplyVersion2Async(connection, cancellationToken).ConfigureAwait(false),
                    2 => await ApplyVersion3Async(connection, cancellationToken).ConfigureAwait(false),
                    _ => throw new InvalidOperationException($"No migration path exists from schema version {version}."),
                };
            }

            initialized = true;
        }
        finally
        {
            initializationGate.Release();
        }
    }

    /// <summary>
    /// Opens a short-lived configured SQLite connection for repository operations.
    /// </summary>
    internal async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            // The busy timeout is intentionally bounded. Later queue/retry policy may add higher-level retries,
            // but a repository operation must never wait forever on a lock.
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

    private static async Task<int> ApplyVersion1Async(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

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

        await SetUserVersionAsync(connection, transaction, 1, cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return 1;
    }

    private static async Task<int> ApplyVersion2Async(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                CREATE TABLE scan_runs (
                    scan_run_id TEXT PRIMARY KEY,
                    source_id TEXT NOT NULL,
                    started_utc TEXT NOT NULL,
                    completed_utc TEXT NULL,
                    outcome TEXT NOT NULL
                        CHECK (outcome IN ('Running', 'Succeeded', 'Partial', 'Failed', 'Cancelled')),
                    issue_count INTEGER NOT NULL DEFAULT 0 CHECK (issue_count >= 0),
                    error_summary TEXT NULL,
                    FOREIGN KEY (source_id) REFERENCES sources(source_id) ON DELETE CASCADE
                );

                CREATE INDEX ix_scan_runs_source_started
                    ON scan_runs(source_id, started_utc);

                CREATE TABLE documents (
                    document_id TEXT PRIMARY KEY,
                    source_id TEXT NOT NULL,
                    canonical_locator TEXT NOT NULL COLLATE NOCASE,
                    display_name TEXT NOT NULL,
                    file_size INTEGER NOT NULL CHECK (file_size >= 0),
                    created_utc TEXT NULL,
                    modified_utc TEXT NOT NULL,
                    last_seen_utc TEXT NOT NULL,
                    last_seen_scan_run_id TEXT NOT NULL,
                    missing_since_utc TEXT NULL,
                    FOREIGN KEY (source_id) REFERENCES sources(source_id) ON DELETE CASCADE,
                    FOREIGN KEY (last_seen_scan_run_id) REFERENCES scan_runs(scan_run_id),
                    UNIQUE (source_id, canonical_locator)
                );

                CREATE INDEX ix_documents_source_last_seen_scan
                    ON documents(source_id, last_seen_scan_run_id);

                CREATE INDEX ix_documents_source_missing
                    ON documents(source_id, missing_since_utc);
                """;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await SetUserVersionAsync(connection, transaction, 2, cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return 2;
    }

    private static async Task<int> ApplyVersion3Async(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                CREATE TABLE crawl_jobs (
                    crawl_job_id TEXT PRIMARY KEY,
                    source_id TEXT NOT NULL,
                    status TEXT NOT NULL
                        CHECK (status IN ('Queued', 'Running', 'Succeeded', 'Partial', 'Failed', 'Cancelled')),
                    requested_utc TEXT NOT NULL,
                    started_utc TEXT NULL,
                    completed_utc TEXT NULL,
                    observed_documents INTEGER NULL CHECK (observed_documents IS NULL OR observed_documents >= 0),
                    non_fatal_issues INTEGER NULL CHECK (non_fatal_issues IS NULL OR non_fatal_issues >= 0),
                    newly_missing_documents INTEGER NULL
                        CHECK (newly_missing_documents IS NULL OR newly_missing_documents >= 0),
                    last_error TEXT NULL,
                    FOREIGN KEY (source_id) REFERENCES sources(source_id) ON DELETE CASCADE
                );

                CREATE INDEX ix_crawl_jobs_source_requested
                    ON crawl_jobs(source_id, requested_utc);

                CREATE TABLE work_items (
                    work_item_id TEXT PRIMARY KEY,
                    crawl_job_id TEXT NOT NULL,
                    source_id TEXT NOT NULL,
                    stage TEXT NOT NULL CHECK (stage IN ('LocalFullScan')),
                    status TEXT NOT NULL CHECK (status IN ('Pending', 'Leased', 'Succeeded', 'Failed')),
                    maximum_depth INTEGER NULL CHECK (maximum_depth IS NULL OR maximum_depth >= 0),
                    attempts INTEGER NOT NULL DEFAULT 0 CHECK (attempts >= 0),
                    next_attempt_utc TEXT NOT NULL,
                    lease_owner TEXT NULL,
                    lease_until_utc TEXT NULL,
                    created_utc TEXT NOT NULL,
                    updated_utc TEXT NOT NULL,
                    last_error TEXT NULL,
                    FOREIGN KEY (crawl_job_id) REFERENCES crawl_jobs(crawl_job_id) ON DELETE CASCADE,
                    FOREIGN KEY (source_id) REFERENCES sources(source_id) ON DELETE CASCADE,
                    UNIQUE (crawl_job_id, stage)
                );

                CREATE INDEX ix_work_items_ready
                    ON work_items(status, next_attempt_utc, created_utc);
                """;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await SetUserVersionAsync(connection, transaction, 3, cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return 3;
    }

    private static async Task SetUserVersionAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        int version,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"PRAGMA user_version = {version};";
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

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
