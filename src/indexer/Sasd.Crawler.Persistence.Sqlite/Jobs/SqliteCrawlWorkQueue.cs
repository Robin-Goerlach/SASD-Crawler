using System.Globalization;
using Microsoft.Data.Sqlite;
using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Application.Jobs;
using Sasd.Crawler.Domain.Crawling;
using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Jobs;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Persistence.Sqlite.Jobs;

/// <summary>
/// SQLite implementation of the durable crawl job/work-item queue.
/// </summary>
/// <remarks>
/// The queue owns state transitions only. Filesystem scanning, extraction and Lucene access remain outside this
/// persistence adapter. One crawl job can now contain one discovery item followed by multiple document-processing
/// items; the job becomes terminal only when every queued processing item has completed or one fails.
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
                    document_id,
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
                    NULL,
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
                options.MaximumDepth.HasValue ? (object)options.MaximumDepth.Value : DBNull.Value);
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
                    document_id,
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
                WHERE crawl_job_id = $jobId
                  AND status IN ('Queued', 'Running');
                """;
            jobCommand.Parameters.AddWithValue("$startedUtc", now);
            jobCommand.Parameters.AddWithValue("$jobId", leased.CrawlJobId.ToString());

            if (await jobCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
            {
                throw new InvalidOperationException(
                    $"Crawl job '{leased.CrawlJobId}' is not runnable while work item '{leased.WorkItemId}' is leased.");
            }
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return leased;
    }

    /// <inheritdoc />
    public async Task CompleteFullScanAsync(
        LeasedCrawlWorkItem workItem,
        LocalSourceScanResult result,
        IReadOnlyCollection<DocumentWorkRequest> followUpWork,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(followUpWork);

        if (workItem.Stage != WorkItemStage.LocalFullScan)
        {
            throw new ArgumentException("Only a LocalFullScan work item can complete discovery.", nameof(workItem));
        }

        if (result.Outcome is not (ScanRunOutcome.Succeeded or ScanRunOutcome.Partial))
        {
            throw new ArgumentException(
                "A completed discovery scan must carry Succeeded or Partial outcome.",
                nameof(result));
        }

        var uniqueFollowUps = followUpWork
            .DistinctBy(item => (item.Stage, item.DocumentId))
            .ToArray();

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await TransitionOwnedWorkItemAsync(
                connection,
                transaction,
                workItem,
                WorkItemStatus.Succeeded,
                completedAtUtc,
                errorSummary: null,
                cancellationToken)
            .ConfigureAwait(false);

        foreach (var followUp in uniqueFollowUps)
        {
            await InsertDocumentWorkAsync(
                    connection,
                    transaction,
                    workItem,
                    followUp,
                    completedAtUtc,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        var finalStatus = result.Outcome == ScanRunOutcome.Partial
            ? CrawlJobStatus.Partial
            : CrawlJobStatus.Succeeded;
        var hasFollowUpWork = uniqueFollowUps.Length > 0;

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
        jobCommand.Parameters.AddWithValue(
            "$status",
            hasFollowUpWork ? CrawlJobStatus.Running.ToString() : finalStatus.ToString());
        jobCommand.Parameters.AddWithValue(
            "$completedUtc",
            hasFollowUpWork ? DBNull.Value : FormatTimestamp(completedAtUtc));
        jobCommand.Parameters.AddWithValue("$observedDocuments", result.ObservedDocuments);
        jobCommand.Parameters.AddWithValue("$nonFatalIssues", result.NonFatalIssues);
        jobCommand.Parameters.AddWithValue("$newlyMissingDocuments", result.NewlyMissingDocuments);
        jobCommand.Parameters.AddWithValue("$jobId", workItem.CrawlJobId.ToString());

        if (await jobCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
        {
            throw new InvalidOperationException(
                $"Crawl job '{workItem.CrawlJobId}' is not Running while discovery completes.");
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CompleteProcessingAsync(
        LeasedCrawlWorkItem workItem,
        DateTimeOffset completedAtUtc,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        if (workItem.Stage == WorkItemStage.LocalFullScan || !workItem.DocumentId.HasValue)
        {
            throw new ArgumentException(
                "Processing completion requires a document-processing work item.",
                nameof(workItem));
        }

        await using var connection = await controlStore.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction =
            (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await TransitionOwnedWorkItemAsync(
                connection,
                transaction,
                workItem,
                WorkItemStatus.Succeeded,
                completedAtUtc,
                errorSummary: null,
                cancellationToken)
            .ConfigureAwait(false);

        await using var remainingCommand = connection.CreateCommand();
        remainingCommand.Transaction = transaction;
        remainingCommand.CommandText = """
            SELECT COUNT(*)
            FROM work_items
            WHERE crawl_job_id = $jobId
              AND status IN ('Pending', 'Leased');
            """;
        remainingCommand.Parameters.AddWithValue("$jobId", workItem.CrawlJobId.ToString());

        var remaining = Convert.ToInt32(
            await remainingCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false),
            CultureInfo.InvariantCulture);

        if (remaining == 0)
        {
            await using var finalizeCommand = connection.CreateCommand();
            finalizeCommand.Transaction = transaction;
            finalizeCommand.CommandText = """
                UPDATE crawl_jobs
                SET
                    status = CASE
                        WHEN COALESCE(non_fatal_issues, 0) > 0 THEN 'Partial'
                        ELSE 'Succeeded'
                    END,
                    completed_utc = $completedUtc,
                    last_error = NULL
                WHERE crawl_job_id = $jobId
                  AND status = 'Running';
                """;
            finalizeCommand.Parameters.AddWithValue("$completedUtc", FormatTimestamp(completedAtUtc));
            finalizeCommand.Parameters.AddWithValue("$jobId", workItem.CrawlJobId.ToString());

            if (await finalizeCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
            {
                throw new InvalidOperationException(
                    $"Crawl job '{workItem.CrawlJobId}' is not Running while its final processing item completes.");
            }
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

        await TransitionOwnedWorkItemAsync(
                connection,
                transaction,
                workItem,
                WorkItemStatus.Failed,
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
              AND status IN ('Queued', 'Running');
            """;
        jobCommand.Parameters.AddWithValue("$completedUtc", FormatTimestamp(completedAtUtc));
        jobCommand.Parameters.AddWithValue("$errorSummary", boundedError);
        jobCommand.Parameters.AddWithValue("$jobId", workItem.CrawlJobId.ToString());

        if (await jobCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
        {
            throw new InvalidOperationException(
                $"Crawl job '{workItem.CrawlJobId}' is not active while its work item fails.");
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

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE work_items
            SET
                status = 'Pending',
                next_attempt_utc = $nextAttemptUtc,
                lease_owner = NULL,
                lease_until_utc = NULL,
                updated_utc = $nextAttemptUtc
            WHERE work_item_id = $workItemId
              AND crawl_job_id = $jobId
              AND status = 'Leased'
              AND lease_owner = $leaseOwner;
            """;
        command.Parameters.AddWithValue("$nextAttemptUtc", FormatTimestamp(nextAttemptUtc));
        command.Parameters.AddWithValue("$workItemId", workItem.WorkItemId.ToString());
        command.Parameters.AddWithValue("$jobId", workItem.CrawlJobId.ToString());
        command.Parameters.AddWithValue("$leaseOwner", workItem.LeaseOwner);

        if (await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
        {
            throw new InvalidOperationException(
                $"Work item '{workItem.WorkItemId}' is no longer leased by '{workItem.LeaseOwner}'.");
        }

        await using var jobCommand = connection.CreateCommand();
        jobCommand.Transaction = transaction;
        jobCommand.CommandText = """
            UPDATE crawl_jobs
            SET
                status = 'Queued',
                completed_utc = NULL
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

    private static async Task InsertDocumentWorkAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        LeasedCrawlWorkItem parentWorkItem,
        DocumentWorkRequest request,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT OR IGNORE INTO work_items (
                work_item_id,
                crawl_job_id,
                source_id,
                document_id,
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
                $documentId,
                $stage,
                'Pending',
                NULL,
                0,
                $createdUtc,
                NULL,
                NULL,
                $createdUtc,
                $createdUtc,
                NULL);
            """;
        command.Parameters.AddWithValue("$workItemId", WorkItemId.New().ToString());
        command.Parameters.AddWithValue("$jobId", parentWorkItem.CrawlJobId.ToString());
        command.Parameters.AddWithValue("$sourceId", parentWorkItem.SourceId.ToString());
        command.Parameters.AddWithValue("$documentId", request.DocumentId.ToString());
        command.Parameters.AddWithValue("$stage", request.Stage.ToString());
        command.Parameters.AddWithValue("$createdUtc", FormatTimestamp(createdAtUtc));

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task TransitionOwnedWorkItemAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        LeasedCrawlWorkItem workItem,
        WorkItemStatus targetStatus,
        DateTimeOffset completedAtUtc,
        string? errorSummary,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE work_items
            SET
                status = $targetStatus,
                lease_owner = NULL,
                lease_until_utc = NULL,
                updated_utc = $completedUtc,
                last_error = $errorSummary
            WHERE work_item_id = $workItemId
              AND crawl_job_id = $jobId
              AND status = 'Leased'
              AND lease_owner = $leaseOwner;
            """;
        command.Parameters.AddWithValue("$targetStatus", targetStatus.ToString());
        command.Parameters.AddWithValue("$completedUtc", FormatTimestamp(completedAtUtc));
        command.Parameters.AddWithValue("$errorSummary", (object?)errorSummary ?? DBNull.Value);
        command.Parameters.AddWithValue("$workItemId", workItem.WorkItemId.ToString());
        command.Parameters.AddWithValue("$jobId", workItem.CrawlJobId.ToString());
        command.Parameters.AddWithValue("$leaseOwner", workItem.LeaseOwner);

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
            reader.IsDBNull(4) ? null : DocumentId.Parse(reader.GetString(4)),
            reader.IsDBNull(5) ? null : reader.GetInt32(5),
            reader.GetInt32(6),
            reader.GetString(7),
            ParseTimestamp(reader.GetString(8)));
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
