using System.Globalization;
using Microsoft.Data.Sqlite;
using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Application.Jobs;
using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Persistence.Sqlite.Jobs;

/// <summary>
/// SQLite implementation of the durable crawl job/work-item queue.
/// </summary>
/// <remarks>
/// The queue uses explicit ownership checks for terminal transitions. A worker can complete, fail or release only
/// the lease it currently owns. Version 0.1 intentionally does not reclaim expired leases automatically because
/// doing so without a completed single-instance/worker-heartbeat policy could create concurrent full scans.
/// </remarks>
public sealed class SqliteCrawlWorkQueue : ICrawlWorkQueue
{
    private readonly SqliteControlStore controlStore;

    /// <summary>Creates a queue for a database path.</summary>
    public SqliteCrawlWorkQueue(string databasePath)
        : this(new SqliteControlStore(databasePath))
    {
    }

    /// <summary>Creates a queue over the shared control-store boundary.</summary>
    public SqliteCrawlWorkQueue(SqliteControlStore controlStore)
    {
        this.controlStore = controlStore ?? throw new ArgumentNullException(nameof(controlStore));
    }

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken)
        => controlStore.InitializeAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<CrawlJobId> EnqueueLocalFullScanAsync(
        SourceId sourceId,
        LocalScanOptions options,
        DateTimeOffset requestedAtUtc,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var jobId = CrawlJobId.New();
        var workItemId = WorkItemId.New();
        var requested = FormatTimestamp(requestedAtUtc);

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await using (var jobCommand = connection.CreateCommand())
        {
            jobCommand.Transaction = transaction;
            jobCommand.CommandText = """
                INSERT INTO crawl_jobs (
                    crawl_job_id,
                    source_id,
                    status,
                    requested_utc)
                VALUES (
                    $jobId,
                    $sourceId,
                    'Queued',
                    $requestedUtc);
                """;
            jobCommand.Parameters.AddWithValue("$jobId", jobId.ToString());
            jobCommand.Parameters.AddWithValue("$sourceId", sourceId.ToString());
            jobCommand.Parameters.AddWithValue("$requestedUtc", requested);

            await jobCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using (var workCommand = connection.CreateCommand())
        {
            workCommand.Transaction = transaction;
            workCommand.CommandText = """
                INSERT INTO work_items (
                    work_item_id,
                    crawl_job_id,
                    source_id,
                    stage,
                    status,
                    maximum_depth,
                    attempts,
                    next_attempt_utc,
                    lease_owner,
                    lease_until_utc,
                    created_utc,
                    updated_utc,
                    last_error)
                VALUES (
                    $workItemId,
                    $jobId,
                    $sourceId,
                    'LocalFullScan',
                    'Pending',
                    $maximumDepth,
                    0,
                    $requestedUtc,
                    NULL,
                    NULL,
                    $requestedUtc,
                    $requestedUtc,
                    NULL);
                """;
            workCommand.Parameters.AddWithValue("$workItemId", workItemId.ToString());
            workCommand.Parameters.AddWithValue("$jobId", jobId.ToString());
            workCommand.Parameters.AddWithValue("$sourceId", sourceId.ToString());
            workCommand.Parameters.AddWithValue(
                "$maximumDepth",
                options.MaximumDepth.HasValue ? options.MaximumDepth.Value : DBNull.Value);
            workCommand.Parameters.AddWithValue("$requestedUtc", requested);

            await workCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return jobId;
    }

    /// <inheritdoc />
    public async Task<LeasedCrawlWorkItem?> TryLeaseNextAsync(
        string leaseOwner,
        DateTimeOffset nowUtc,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(leaseOwner))
        {
            throw new ArgumentException("A lease owner is required.", nameof(leaseOwner));
        }

        if (leaseDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(leaseDuration), "Lease duration must be positive.");
        }

        var now = FormatTimestamp(nowUtc);
        var leaseUntil = FormatTimestamp(nowUtc.Add(leaseDuration));

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        LeasedCrawlWorkItem? leased = null;

        await using (var leaseCommand = connection.CreateCommand())
        {
            leaseCommand.Transaction = transaction;

            // UPDATE ... RETURNING makes selection and ownership transition one SQLite statement. Two workers cannot
            // both observe the same Pending row and successfully lease it.
            leaseCommand.CommandText = """
                UPDATE work_items
                SET
                    status = 'Leased',
                    attempts = attempts + 1,
                    lease_owner = $leaseOwner,
                    lease_until_utc = $leaseUntilUtc,
                    updated_utc = $nowUtc
                WHERE work_item_id = (
                    SELECT work_item_id
                    FROM work_items
                    WHERE status = 'Pending'
                      AND next_attempt_utc <= $nowUtc
                    ORDER BY created_utc, work_item_id
                    LIMIT 1
                )
                RETURNING
                    work_item_id,
                    crawl_job_id,
                    source_id,
                    stage,
                    maximum_depth,
                    attempts,
                    lease_owner,
                    lease_until_utc;
                """;
            leaseCommand.Parameters.AddWithValue("$leaseOwner", leaseOwner);
            leaseCommand.Parameters.AddWithValue("$leaseUntilUtc", leaseUntil);
            leaseCommand.Parameters.AddWithValue("$nowUtc", now);

            await using var reader = await leaseCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                leased = ReadLease(reader);
            }
        }

        if (leased is null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return null;
        }

        await using (var jobCommand = connection.CreateCommand())
        {
            jobCommand.Transaction = transaction;
            jobCommand.CommandText = """
                UPDATE crawl_jobs
                SET
                    status = 'Running',
                    started_utc = COALESCE(started_utc, $startedUtc),
                    completed_utc = NULL,
                    last_error = NULL
                WHERE crawl_job_id = $jobId;
                """;
            jobCommand.Parameters.AddWithValue("$startedUtc", now);
            jobCommand.Parameters.AddWithValue("$jobId", leased.CrawlJobId.ToString());

            if (await jobCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
            {
                throw new InvalidOperationException(
                    $"Crawl job '{leased.CrawlJobId}' disappeared while its work item was being leased.");
            }
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return leased;
    }

    /// <inheritdoc />
    public async Task MarkCompletedAsync(
        LeasedCrawlWorkItem workItem,
        LocalSourceScanResult result,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        ArgumentNullException.ThrowIfNull(result);

        if (result.Outcome is not (ScanRunOutcome.Succeeded or ScanRunOutcome.Partial))
        {
            throw new ArgumentException(
                "A completed crawl work item must carry a Succeeded or Partial scan outcome.",
                nameof(result));
        }

        var jobStatus = result.Outcome == ScanRunOutcome.Succeeded
            ? CrawlJobStatus.Succeeded
            : CrawlJobStatus.Partial;

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await EnsureOwnedTransitionAsync(
                connection,
                transaction,
                workItem,
                """
                UPDATE work_items
                SET
                    status = 'Succeeded',
                    lease_owner = NULL,
                    lease_until_utc = NULL,
                    updated_utc = $completedUtc,
                    last_error = NULL
                WHERE work_item_id = $workItemId
                  AND crawl_job_id = $jobId
                  AND status = 'Leased'
                  AND lease_owner = $leaseOwner;
                """,
                completedAtUtc,
                errorSummary: null,
                cancellationToken)
            .ConfigureAwait(false);

        await using var jobCommand = connection.CreateCommand();
        jobCommand.Transaction = transaction;
        jobCommand.CommandText = """
            UPDATE crawl_jobs
            SET
                status = $status,
                completed_utc = $completedUtc,
                observed_documents = $observedDocuments,
                non_fatal_issues = $nonFatalIssues,
                newly_missing_documents = $newlyMissingDocuments,
                last_error = NULL
            WHERE crawl_job_id = $jobId
              AND status = 'Running';
            """;
        jobCommand.Parameters.AddWithValue("$status", jobStatus.ToString());
        jobCommand.Parameters.AddWithValue("$completedUtc", FormatTimestamp(completedAtUtc));
        jobCommand.Parameters.AddWithValue("$observedDocuments", result.ObservedDocuments);
        jobCommand.Parameters.AddWithValue("$nonFatalIssues", result.NonFatalIssues);
        jobCommand.Parameters.AddWithValue("$newlyMissingDocuments", result.NewlyMissingDocuments);
        jobCommand.Parameters.AddWithValue("$jobId", workItem.CrawlJobId.ToString());

        if (await jobCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
        {
            throw new InvalidOperationException(
                $"Crawl job '{workItem.CrawlJobId}' is not Running while its leased work item completes.");
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task MarkFailedAsync(
        LeasedCrawlWorkItem workItem,
        string errorSummary,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        if (string.IsNullOrWhiteSpace(errorSummary))
        {
            throw new ArgumentException("A failure summary is required.", nameof(errorSummary));
        }

        var boundedError = BoundError(errorSummary);

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await EnsureOwnedTransitionAsync(
                connection,
                transaction,
                workItem,
                """
                UPDATE work_items
                SET
                    status = 'Failed',
                    lease_owner = NULL,
                    lease_until_utc = NULL,
                    updated_utc = $completedUtc,
                    last_error = $errorSummary
                WHERE work_item_id = $workItemId
                  AND crawl_job_id = $jobId
                  AND status = 'Leased'
                  AND lease_owner = $leaseOwner;
                """,
                completedAtUtc,
                boundedError,
                cancellationToken)
            .ConfigureAwait(false);

        await using var jobCommand = connection.CreateCommand();
        jobCommand.Transaction = transaction;
        jobCommand.CommandText = """
            UPDATE crawl_jobs
            SET
                status = 'Failed',
                completed_utc = $completedUtc,
                last_error = $errorSummary
            WHERE crawl_job_id = $jobId
              AND status = 'Running';
            """;
        jobCommand.Parameters.AddWithValue("$completedUtc", FormatTimestamp(completedAtUtc));
        jobCommand.Parameters.AddWithValue("$errorSummary", boundedError);
        jobCommand.Parameters.AddWithValue("$jobId", workItem.CrawlJobId.ToString());

        if (await jobCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
        {
            throw new InvalidOperationException(
                $"Crawl job '{workItem.CrawlJobId}' is not Running while its leased work item fails.");
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ReleaseLeaseAsync(
        LeasedCrawlWorkItem workItem,
        DateTimeOffset nextAttemptUtc,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await EnsureOwnedTransitionAsync(
                connection,
                transaction,
                workItem,
                """
                UPDATE work_items
                SET
                    status = 'Pending',
                    next_attempt_utc = $completedUtc,
                    lease_owner = NULL,
                    lease_until_utc = NULL,
                    updated_utc = $completedUtc
                WHERE work_item_id = $workItemId
                  AND crawl_job_id = $jobId
                  AND status = 'Leased'
                  AND lease_owner = $leaseOwner;
                """,
                nextAttemptUtc,
                errorSummary: null,
                cancellationToken)
            .ConfigureAwait(false);

        await using var jobCommand = connection.CreateCommand();
        jobCommand.Transaction = transaction;
        jobCommand.CommandText = """
            UPDATE crawl_jobs
            SET
                status = 'Queued',
                completed_utc = NULL,
                last_error = NULL
            WHERE crawl_job_id = $jobId
              AND status = 'Running';
            """;
        jobCommand.Parameters.AddWithValue("$jobId", workItem.CrawlJobId.ToString());

        if (await jobCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
        {
            throw new InvalidOperationException(
                $"Crawl job '{workItem.CrawlJobId}' is not Running while its lease is released.");
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CrawlJobSnapshot?> GetJobAsync(
        CrawlJobId jobId,
        CancellationToken cancellationToken)
    {
        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                crawl_job_id,
                source_id,
                status,
                requested_utc,
                started_utc,
                completed_utc,
                observed_documents,
                non_fatal_issues,
                newly_missing_documents,
                last_error
            FROM crawl_jobs
            WHERE crawl_job_id = $jobId
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$jobId", jobId.ToString());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false)
            ? ReadJob(reader)
            : null;
    }

    private static async Task EnsureOwnedTransitionAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        LeasedCrawlWorkItem workItem,
        string sql,
        DateTimeOffset transitionAtUtc,
        string? errorSummary,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.Parameters.AddWithValue("$completedUtc", FormatTimestamp(transitionAtUtc));
        command.Parameters.AddWithValue("$workItemId", workItem.WorkItemId.ToString());
        command.Parameters.AddWithValue("$jobId", workItem.CrawlJobId.ToString());
        command.Parameters.AddWithValue("$leaseOwner", workItem.LeaseOwner);

        if (sql.Contains("$errorSummary", StringComparison.Ordinal))
        {
            command.Parameters.AddWithValue("$errorSummary", (object?)errorSummary ?? DBNull.Value);
        }

        if (await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
        {
            throw new InvalidOperationException(
                $"Work item '{workItem.WorkItemId}' is no longer leased by '{workItem.LeaseOwner}'.");
        }
    }

    private static LeasedCrawlWorkItem ReadLease(SqliteDataReader reader)
    {
        var stageText = reader.GetString(3);
        if (!Enum.TryParse<WorkItemStage>(stageText, ignoreCase: false, out var stage))
        {
            throw new InvalidDataException($"Unknown persisted work-item stage '{stageText}'.");
        }

        return new LeasedCrawlWorkItem(
            WorkItemId.Parse(reader.GetString(0)),
            CrawlJobId.Parse(reader.GetString(1)),
            SourceId.Parse(reader.GetString(2)),
            stage,
            reader.IsDBNull(4) ? null : reader.GetInt32(4),
            reader.GetInt32(5),
            reader.GetString(6),
            ParseTimestamp(reader.GetString(7)));
    }

    private static CrawlJobSnapshot ReadJob(SqliteDataReader reader)
    {
        var statusText = reader.GetString(2);
        if (!Enum.TryParse<CrawlJobStatus>(statusText, ignoreCase: false, out var status))
        {
            throw new InvalidDataException($"Unknown persisted crawl-job status '{statusText}'.");
        }

        return new CrawlJobSnapshot(
            CrawlJobId.Parse(reader.GetString(0)),
            SourceId.Parse(reader.GetString(1)),
            status,
            ParseTimestamp(reader.GetString(3)),
            reader.IsDBNull(4) ? null : ParseTimestamp(reader.GetString(4)),
            reader.IsDBNull(5) ? null : ParseTimestamp(reader.GetString(5)),
            reader.IsDBNull(6) ? null : reader.GetInt32(6),
            reader.IsDBNull(7) ? null : reader.GetInt32(7),
            reader.IsDBNull(8) ? null : reader.GetInt32(8),
            reader.IsDBNull(9) ? null : reader.GetString(9));
    }

    private static string BoundError(string value)
    {
        const int maximumLength = 1024;
        var trimmed = value.Trim();
        return trimmed.Length <= maximumLength ? trimmed : trimmed[..maximumLength];
    }

    private static string FormatTimestamp(DateTimeOffset value)
        => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset ParseTimestamp(string value)
        => DateTimeOffset.Parse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);
}
