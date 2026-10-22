using System.Globalization;
using Microsoft.Data.Sqlite;
using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Persistence.Sqlite;

namespace Sasd.Crawler.Persistence.Sqlite.Crawling;

/// <summary>
/// SQLite implementation of the durable document registry and scan-run state.
/// </summary>
/// <remarks>
/// Reconciliation is deliberately performed inside the same transaction that marks an authoritative scan
/// successful. A partial/failed/cancelled run never executes the missing-document update.
/// </remarks>
public sealed class SqliteDocumentRegistry : IDocumentRegistry
{
    private readonly SqliteControlStore controlStore;

    /// <summary>Creates a registry for a database path.</summary>
    public SqliteDocumentRegistry(string databasePath)
        : this(new SqliteControlStore(databasePath))
    {
    }

    /// <summary>Creates a registry over the shared control-store boundary.</summary>
    public SqliteDocumentRegistry(SqliteControlStore controlStore)
    {
        this.controlStore = controlStore ?? throw new ArgumentNullException(nameof(controlStore));
    }

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken)
        => controlStore.InitializeAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<ScanRunId> BeginFullScanAsync(
        SourceId sourceId,
        DateTimeOffset startedAtUtc,
        CancellationToken cancellationToken)
    {
        var scanRunId = ScanRunId.New();

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO scan_runs (
                scan_run_id,
                source_id,
                started_utc,
                completed_utc,
                outcome,
                issue_count,
                error_summary)
            VALUES (
                $scanRunId,
                $sourceId,
                $startedUtc,
                NULL,
                'Running',
                0,
                NULL);
            """;
        command.Parameters.AddWithValue("$scanRunId", scanRunId.ToString());
        command.Parameters.AddWithValue("$sourceId", sourceId.ToString());
        command.Parameters.AddWithValue("$startedUtc", FormatTimestamp(startedAtUtc));

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return scanRunId;
    }

    /// <inheritdoc />
    public async Task RecordSeenAsync(
        ScanRunId scanRunId,
        SourceId sourceId,
        FileObservation observation,
        DateTimeOffset observedAtUtc,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(observation);

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        if (!await IsRunningScanAsync(connection, transaction, scanRunId, sourceId, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new InvalidOperationException(
                $"Scan '{scanRunId}' is not an active run for source '{sourceId}'.");
        }

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO documents (
                document_id,
                source_id,
                canonical_locator,
                display_name,
                file_size,
                created_utc,
                modified_utc,
                last_seen_utc,
                last_seen_scan_run_id,
                missing_since_utc)
            VALUES (
                $documentId,
                $sourceId,
                $canonicalLocator,
                $displayName,
                $fileSize,
                $createdUtc,
                $modifiedUtc,
                $lastSeenUtc,
                $scanRunId,
                NULL)
            ON CONFLICT(source_id, canonical_locator) DO UPDATE SET
                display_name = excluded.display_name,
                file_size = excluded.file_size,
                created_utc = excluded.created_utc,
                modified_utc = excluded.modified_utc,
                last_seen_utc = excluded.last_seen_utc,
                last_seen_scan_run_id = excluded.last_seen_scan_run_id,
                missing_since_utc = NULL;
            """;

        command.Parameters.AddWithValue("$documentId", DocumentId.New().ToString());
        command.Parameters.AddWithValue("$sourceId", sourceId.ToString());
        command.Parameters.AddWithValue("$canonicalLocator", observation.CanonicalLocator);
        command.Parameters.AddWithValue("$displayName", observation.DisplayName);
        command.Parameters.AddWithValue("$fileSize", observation.FileSize);
        command.Parameters.AddWithValue(
            "$createdUtc",
            observation.CreatedAtUtc.HasValue
                ? (object)FormatTimestamp(observation.CreatedAtUtc.Value)
                : DBNull.Value);
        command.Parameters.AddWithValue("$modifiedUtc", FormatTimestamp(observation.ModifiedAtUtc));
        command.Parameters.AddWithValue("$lastSeenUtc", FormatTimestamp(observedAtUtc));
        command.Parameters.AddWithValue("$scanRunId", scanRunId.ToString());

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ScanCompletionResult> CompleteFullScanAsync(
        ScanRunId scanRunId,
        SourceId sourceId,
        ScanRunOutcome outcome,
        int issueCount,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken)
    {
        ValidateCompletion(outcome, issueCount);

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var completed = await CompleteRunningScanAsync(
                connection,
                transaction,
                scanRunId,
                sourceId,
                outcome,
                issueCount,
                errorSummary: null,
                completedAtUtc: completedAtUtc,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (!completed)
        {
            throw new InvalidOperationException(
                $"Scan '{scanRunId}' is not an active run for source '{sourceId}'.");
        }

        var newlyMissing = 0;
        if (outcome == ScanRunOutcome.Succeeded)
        {
            // This statement is the delete-authority boundary. It is impossible to reach for Partial/Failed/
            // Cancelled runs through this implementation, so an incomplete traversal cannot become mass deletion.
            await using var reconcile = connection.CreateCommand();
            reconcile.Transaction = transaction;
            reconcile.CommandText = """
                UPDATE documents
                SET missing_since_utc = $completedUtc
                WHERE source_id = $sourceId
                  AND last_seen_scan_run_id <> $scanRunId
                  AND missing_since_utc IS NULL;
                """;
            reconcile.Parameters.AddWithValue("$completedUtc", FormatTimestamp(completedAtUtc));
            reconcile.Parameters.AddWithValue("$sourceId", sourceId.ToString());
            reconcile.Parameters.AddWithValue("$scanRunId", scanRunId.ToString());

            newlyMissing = await reconcile.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new ScanCompletionResult(newlyMissing);
    }

    /// <inheritdoc />
    public async Task AbortFullScanAsync(
        ScanRunId scanRunId,
        SourceId sourceId,
        ScanRunOutcome outcome,
        int issueCount,
        string? errorSummary,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken)
    {
        if (outcome is not (ScanRunOutcome.Failed or ScanRunOutcome.Cancelled))
        {
            throw new ArgumentOutOfRangeException(
                nameof(outcome),
                "Abort may only persist Failed or Cancelled scan outcomes.");
        }

        if (issueCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(issueCount), "Issue count must not be negative.");
        }

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var completed = await CompleteRunningScanAsync(
                connection,
                transaction,
                scanRunId,
                sourceId,
                outcome,
                issueCount: issueCount,
                errorSummary: errorSummary,
                completedAtUtc: completedAtUtc,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (!completed)
        {
            throw new InvalidOperationException(
                $"Scan '{scanRunId}' is not an active run for source '{sourceId}'.");
        }

        // No document UPDATE appears in this code path by design.
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DocumentRecord>> ListDocumentsAsync(
        SourceId sourceId,
        CancellationToken cancellationToken)
    {
        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                document_id,
                source_id,
                canonical_locator,
                display_name,
                file_size,
                created_utc,
                modified_utc,
                last_seen_utc,
                last_seen_scan_run_id,
                missing_since_utc
            FROM documents
            WHERE source_id = $sourceId
            ORDER BY canonical_locator COLLATE NOCASE;
            """;
        command.Parameters.AddWithValue("$sourceId", sourceId.ToString());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var documents = new List<DocumentRecord>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            documents.Add(ReadDocument(reader));
        }

        return documents;
    }

    private static void ValidateCompletion(ScanRunOutcome outcome, int issueCount)
    {
        if (issueCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(issueCount), "Issue count must not be negative.");
        }

        if (outcome == ScanRunOutcome.Succeeded && issueCount != 0)
        {
            throw new ArgumentException(
                "An authoritative successful scan cannot contain unresolved scan issues.",
                nameof(issueCount));
        }

        if (outcome == ScanRunOutcome.Partial && issueCount == 0)
        {
            throw new ArgumentException(
                "A partial scan must record at least one issue.",
                nameof(issueCount));
        }

        if (outcome is not (ScanRunOutcome.Succeeded or ScanRunOutcome.Partial))
        {
            throw new ArgumentOutOfRangeException(
                nameof(outcome),
                "Completion may only persist Succeeded or Partial scan outcomes.");
        }
    }

    private static async Task<bool> IsRunningScanAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        ScanRunId scanRunId,
        SourceId sourceId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT COUNT(*)
            FROM scan_runs
            WHERE scan_run_id = $scanRunId
              AND source_id = $sourceId
              AND outcome = 'Running';
            """;
        command.Parameters.AddWithValue("$scanRunId", scanRunId.ToString());
        command.Parameters.AddWithValue("$sourceId", sourceId.ToString());

        var value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(value, CultureInfo.InvariantCulture) == 1;
    }

    private static async Task<bool> CompleteRunningScanAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        ScanRunId scanRunId,
        SourceId sourceId,
        ScanRunOutcome outcome,
        int issueCount,
        string? errorSummary,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE scan_runs
            SET
                completed_utc = $completedUtc,
                outcome = $outcome,
                issue_count = $issueCount,
                error_summary = $errorSummary
            WHERE scan_run_id = $scanRunId
              AND source_id = $sourceId
              AND outcome = 'Running';
            """;
        command.Parameters.AddWithValue("$completedUtc", FormatTimestamp(completedAtUtc));
        command.Parameters.AddWithValue("$outcome", outcome.ToString());
        command.Parameters.AddWithValue("$issueCount", issueCount);
        command.Parameters.AddWithValue("$errorSummary", (object?)errorSummary ?? DBNull.Value);
        command.Parameters.AddWithValue("$scanRunId", scanRunId.ToString());
        command.Parameters.AddWithValue("$sourceId", sourceId.ToString());

        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) == 1;
    }

    private static DocumentRecord ReadDocument(SqliteDataReader reader)
        => DocumentRecord.Restore(
            DocumentId.Parse(reader.GetString(0)),
            SourceId.Parse(reader.GetString(1)),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetInt64(4),
            reader.IsDBNull(5) ? null : ParseTimestamp(reader.GetString(5)),
            ParseTimestamp(reader.GetString(6)),
            ParseTimestamp(reader.GetString(7)),
            ScanRunId.Parse(reader.GetString(8)),
            reader.IsDBNull(9) ? null : ParseTimestamp(reader.GetString(9)));

    private static string FormatTimestamp(DateTimeOffset value)
        => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset ParseTimestamp(string value)
        => DateTimeOffset.Parse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);
}
